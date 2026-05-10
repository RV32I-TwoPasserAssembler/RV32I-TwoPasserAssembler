.text
.global bekle_fonksiyonu

bekle_fonksiyonu:
    addi x5, x0, 2000    # Dış döngü: 2000

dis_dongu:
    addi x6, x0, 2000    # İç döngü: 2000

ic_dongu:
    addi x6, x6, -1
    bne x6, x0, ic_dongu # İç döngü bitene kadar dön

    addi x5, x5, -1
    bne x5, x0, dis_dongu # Dış döngü bitene kadar dön

    jalr x0, x1, 0       # Çağrıldığı yere (main.s) geri dön