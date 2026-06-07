.text
.org 0x0000

.global start
.extern bekle_fonksiyonu

start:
    addi x4, x0, 1024   # LED bellek adresini (0x400) x4'e sabitle
    
    # --- 1. ADIM (En Dış LED'ler: 1 ve 6) ---
    addi x7, x0, 33     # Veri: 33 (Binary: 100001)
    sw x7, x4, 0        # Donanıma yaz ve yak
    jal x1, bekle_fonksiyonu  

    # --- 2. ADIM (Orta LED'ler: 2 ve 5) ---
    addi x7, x0, 18     # Veri: 18 (Binary: 010010)
    sw x7, x4, 0       
    jal x1, bekle_fonksiyonu

    # --- 3. ADIM (En İç LED'ler: 3 ve 4) ---
    addi x7, x0, 12     # Veri: 12 (Binary: 001100)
    sw x7, x4, 0       
    jal x1, bekle_fonksiyonu

    # --- BAŞA SAR ---
    jal x0, start       # Sonsuz döngü, tekrar 1. adıma zıpla

.data
.word 0