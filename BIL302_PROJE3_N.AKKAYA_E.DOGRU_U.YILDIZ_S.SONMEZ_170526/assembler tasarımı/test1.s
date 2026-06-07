# test1.s - Temel I/O ve Döngü Testi
.text
      

.global start
start:
    addi x4, x0, 512    # DÜZELTİLDİ: LED Adresi artık 512
    addi x7, x0, 31     # 5 LED'i yak (5. LED kalp atışına bağlı)
    addi x8, x0, 0      

sonsuz_dongu:
    sw x7, x4, 0        
    
    addi x5, x0, 2000
dis_dongu:
    addi x6, x0, 2000
ic_dongu:
    addi x6, x6, -1
    bne x6, x0, ic_dongu
    addi x5, x5, -1
    bne x5, x0, dis_dongu

    sw x8, x4, 0        
    
    addi x5, x0, 2000
dis_dongu2:
    addi x6, x0, 2000
ic_dongu2:
    addi x6, x6, -1
    bne x6, x0, ic_dongu2
    addi x5, x5, -1
    bne x5, x0, dis_dongu2

    jal x0, sonsuz_dongu