.text
.org 0x0000

.global start
.extern bekle_fonksiyonu

start:
    addi x4, x0, 1024   # Verilog'da ayarladığın LED hafıza adresi (0x400 = 1024)
    addi x7, x0, 255    # 255 (Binary: 11111111) -> Verilog tersleyip 0 yapacak, LED'ler YANACAK
    addi x8, x0, 0      # 0 (Binary: 00000000) -> Verilog tersleyip 1 yapacak, LED'ler SÖNECEK

sonsuz_dongu:
    sw x7, x4, 0        # LED'leri YAK
    jal x1, bekle_fonksiyonu   # Gecikme fonksiyonuna git

    sw x8, x4, 0        # LED'leri SÖNDÜR
    jal x1, bekle_fonksiyonu   # Gecikme fonksiyonuna git

    jal x0, sonsuz_dongu # Başa dön ve sonsuza kadar tekrarla

.data
.word 0