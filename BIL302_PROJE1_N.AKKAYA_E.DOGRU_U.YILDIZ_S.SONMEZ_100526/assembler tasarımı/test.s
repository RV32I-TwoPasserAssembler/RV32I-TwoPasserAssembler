# PicoRV Test Programı
.org 0x0000

start:
    addi x1, x0, 10    # x1 = 10
    addi x2, x0, 20    # x2 = 20
    add x3, x1, x2     # x3 = 30 (0x1E)

loop:
    addi x1, x1, 1     # x1'i 1 artır
    bne x1, x2, loop   # x1, x2'ye eşit değilse loop'a dallan

end:
    sw x3, x0, 0       # Sonucu belleğe yaz