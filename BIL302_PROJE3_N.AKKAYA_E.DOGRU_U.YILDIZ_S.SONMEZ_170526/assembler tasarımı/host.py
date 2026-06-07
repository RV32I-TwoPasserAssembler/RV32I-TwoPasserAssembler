import serial
import time
import sys

COM_PORT = 'COM10'
BAUDRATE = 115200
FILE_PATH = 'program.mi'
MAX_RETRIES = 3 

def main():
    print("===========================================")
    print(" PicoRV32 Endüstriyel Yükleyici ")
    print("===========================================")
    
    try:
        with open(FILE_PATH, 'r') as f:
            lines = f.readlines()
    except FileNotFoundError:
        print(f"HATA: '{FILE_PATH}' dosyası bulunamadı!")
        return

    program_bytes = bytearray()
    start_address = None
    current_address = None

    for line in lines:
        line = line.strip()
        if not line: continue
        
        # --- ÇOKLU ADRES VE PADDING (BOŞLUK DOLDURMA) YÖNETİMİ ---
        if line.startswith('@'): 
            target_addr = int(line[1:], 16) * 4 # Word adresini Byte adresine çevir
            
            # 1. Gelen ilk adresi başlangıç kabul et
            if start_address is None:
                start_address = target_addr
                current_address = target_addr
            # 2. Eğer yeni bir bölge geldiyse ve arada boşluk varsa 0x00 ile doldur
            else:
                if target_addr > current_address:
                    padding_size = target_addr - current_address
                    program_bytes.extend(b'\x00' * padding_size)
                    current_address = target_addr
            continue 
        
        # Güvenlik Ağı: Eğer dosyada hiç '@' yoksa 1024'ten (0x0400) başlat
        if start_address is None:
            start_address = 1024
            current_address = 1024

        word = int(line, 16)
        program_bytes.append(word & 0xFF)
        program_bytes.append((word >> 8) & 0xFF)
        program_bytes.append((word >> 16) & 0xFF)
        program_bytes.append((word >> 24) & 0xFF)
        current_address += 4 # Her komutta güncel adresi 4 bayt ileri taşı

    dosya_boyutu = len(program_bytes)
    # --- YENİ: FİZİKSEL BELLEK (BRAM) SINIR KONTROLÜ ---
    MEM_SIZE = 8192
    if start_address + dosya_boyutu > MEM_SIZE:
        print(f"\n[HATA] Bellek Taşması! Program (Adres: 0x{start_address:04X} + Boyut: {dosya_boyutu} bayt) FPGA RAM sınırını ({MEM_SIZE} bayt) aşıyor.")
        return
    
    # 1. 32-Bit Boyut Paketlemesi
    size_bytes = [
        dosya_boyutu & 0xFF,
        (dosya_boyutu >> 8) & 0xFF,
        (dosya_boyutu >> 16) & 0xFF,
        (dosya_boyutu >> 24) & 0xFF
    ]

    # 2. 32-Bit Adres Paketlemesi
    addr_bytes = [
        start_address & 0xFF, 
        (start_address >> 8) & 0xFF,
        (start_address >> 16) & 0xFF, 
        (start_address >> 24) & 0xFF
    ]

    # 3. Kusursuz Tam Paket Checksum
    # Boyut byte'ları + Adres byte'ları + Kod byte'ları
    checksum = (sum(size_bytes) + sum(addr_bytes) + sum(program_bytes)) % 256

    print(f"[BİLGİ] Adres: 0x{start_address:04X} | Boyut: {dosya_boyutu} bayt | Tam Paket Checksum: {hex(checksum)}")

    try:
        ser = serial.Serial(COM_PORT, BAUDRATE, timeout=2)
        time.sleep(2) 
        
        for deneme in range(1, MAX_RETRIES + 1):
            print(f"\n--- [YÜKLEME DENEMESİ {deneme}/{MAX_RETRIES}] ---")
            if deneme > 1: time.sleep(0.5)

            # --- SIFIR KAYIPLI GÖNDERİM PROTOKOLÜ ---
            for b in size_bytes: ser.write(bytes([b]))    # 4 Byte Boyut
            for b in addr_bytes: ser.write(bytes([b]))    # 4 Byte Adres
            for b in program_bytes: ser.write(bytes([b])) # N Byte Veri (Sıfırlarla doldurulmuş tek parça)
            ser.write(bytes([checksum]))                  # 1 Byte Checksum
            
            cevap = ser.read(1)
            
            if not cevap:
                print("[UYARI] Zaman Aşımı! FPGA yanıt vermedi (Kablo/Reset Kontrolü).")
            elif cevap[0] == 0x06:
                print(f" -> [OK] BAŞARILI: FPGA ACK Gönderdi! {dosya_boyutu} bayt doğrulandı.")
                break 
            elif cevap[0] == 0x15:
                print(" -> [FAIL] HATA: FPGA NACK Gönderdi! Paket bozulmuş, tekrar deneniyor...")
            else:
                print(f" -> [?] BİLİNMEYEN YANIT ALINDI: {hex(cevap[0])}")

            if deneme == MAX_RETRIES:
                print("\n[İPTAL] Maksimum deneme sayısına ulaşıldı. Yükleme BAŞARISIZ!")

        ser.close()
        
    except serial.SerialException:
        print(f"\nHATA: {COM_PORT} açılamadı!")

if __name__ == '__main__':
    main()