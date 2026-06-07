# test2.s - 6 LED'li Tam Sürüm Karaşimşek (Release Modu)
.text
       

.global start
start:
    addi x4, x0, 512    # LED donanım adresi

basa_don:
    addi x7, x0, 1      # Başlangıçta sadece 0. LED yanacak

kaydir:
    sw x7, x4, 0        # LED'i yak

    # Gecikme (Hız) Döngüsü - Hızı artırmak veya azaltmak istersen 1500 sayılarını değiştirebilirsin
    addi x5, x0, 1500
dis:
    addi x6, x0, 1500
ic:
    addi x6, x6, -1
    bne x6, x0, ic
    addi x5, x5, -1
    bne x5, x0, dis

    # Matematiksel İşlem: Sayıyı kendisiyle toplayarak x2 yap (Sola Kaydır)
    add x7, x7, x7      
    
    # Sınır kontrolü (Sayı 64 olunca başa dön, çünkü tam 6 LED kullanıyoruz)
    addi x8, x0, 64
    beq x7, x8, basa_don
    
    jal x0, kaydir