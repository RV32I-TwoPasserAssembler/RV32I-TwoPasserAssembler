# gecikme.s - düzeltilmiş
.text
.global bekle_fonksiyonu

bekle_fonksiyonu:
    addi x5, x0, 2000

dis_dongu:
    addi x6, x0, 2000

ic_dongu:
    addi x6, x6, -1
    bne x6, x0, ic_dongu

    addi x5, x5, -1
    bne x5, x0, dis_dongu

    jalr x0, x1, 0    # x1'deki adrese geri dön (çağıran kim olursa)