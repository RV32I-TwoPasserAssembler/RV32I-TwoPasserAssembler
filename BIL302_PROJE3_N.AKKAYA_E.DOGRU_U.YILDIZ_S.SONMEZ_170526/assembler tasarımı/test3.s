# test3.s - Fonksiyon / Alt Program Çağrısı Testi (Flaşör Efekti)
.text

     

.global start
start:
    addi x4, x0, 512    # LED adresi

sonsuz_dongu:
    # Desen 1: İçteki iki LED'i yak (0110 binary -> 6)
    addi x7, x0, 6
    sw x7, x4, 0
    jal x1, bekle_fonksiyonu  # Fonksiyona zıpla ve dönüş adresini x1'e kaydet

    # Desen 2: Dıştaki iki LED'i yak (1001 binary -> 9)
    addi x7, x0, 9
    sw x7, x4, 0
    jal x1, bekle_fonksiyonu  # Fonksiyona tekrar zıpla

    jal x0, sonsuz_dongu      # Döngüyü başa sar

# --- ALT PROGRAM (BEKLEME FONKSİYONU) ---
bekle_fonksiyonu:
    addi x5, x0, 1500
dis:
    addi x6, x0, 1500
ic:
    addi x6, x6, -1
    bne x6, x0, ic
    addi x5, x5, -1
    bne x5, x0, dis
    
    jalr x0, x1, 0            # x1'de kayıtlı olan adrese geri dön!