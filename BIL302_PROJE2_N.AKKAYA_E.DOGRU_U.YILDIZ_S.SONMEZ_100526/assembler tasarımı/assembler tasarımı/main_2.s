.text
.org 0x0000

.global start
.extern bekle_fonksiyonu

start:
   addi x4, x0, 1024   
    addi x9, x0, 64

baslangic:
    addi x7,x0,1
         
kaydirma_dongusu:
    sw x7,x4,0
    jal x1, bekle_fonksiyonu
    add x7,x7,x7
    beq x7,x9,baslangic

    jal x0, kaydirma_dongusu


    
      

.data
.word 0