# main.s - düzeltilmiş
.text
.org 0x0000

.global start
.extern bekle_fonksiyonu

start:
    addi x4, x0, 512   # LED adresi
    addi x7, x0, 255    # açık değeri
    addi x8, x0, 0      # kapalı değeri

sonsuz_dongu:
    sw x7, x4, 0         # LED YAK
    jal x1, bekle_fonksiyonu   # x1'e dönüş adresi kaydedilir

    sw x8, x4, 0         # LED SÖNDÜR
    jal x1, bekle_fonksiyonu   # x1'e dönüş adresi kaydedilir

    jal x0, sonsuz_dongu  # başa dön

.data
.word 0