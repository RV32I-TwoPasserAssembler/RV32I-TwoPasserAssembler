# Kaynak: Patterson & Hennessy, RISC-V Edition (Array Initialization & Procedure Call)
.text
.org 0x0000

.global start
.extern process_data

start:
    addi x10, x0, 5      # Döngü sayacı (N = 5 iterasyon)
    addi x11, x0, 2048   # Verinin yazılacağı RAM taban adresi (0x0800 = 2048)
    addi x12, x0, 0      # İşlenecek başlangıç değeri (Accumulator)

loop:
    beq x10, x0, end     # Sayaç 0 ise döngüden çık (B-Type test)

    jal x1, process_data # Harici fonksiyona git ve x1'e dönüş adresini kaydet (J-Type ve Extern test)

    sw x13, x11, 0       # process_data'dan x13 içinde dönen sonucu belleğe yaz (S-Type test)
                         # Not: Assembler'ınız "sw rs2, rs1, imm" formatında çalışıyor.
    
    addi x11, x11, 4     # Bellek adresini bir word (4 byte) artır (I-Type test)
    addi x10, x10, -1    # Sayacı 1 azalt
    
    jal x0, loop         # Başa dön (J-Type pseudo-branch test)

end:
    jal x0, end          # Programı güvenli bir şekilde sonsuz döngüde bitir

.data
.word 0