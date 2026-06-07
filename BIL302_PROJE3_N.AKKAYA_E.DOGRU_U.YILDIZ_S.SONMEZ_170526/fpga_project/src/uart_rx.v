module uart_rx #(
    parameter CLK_FREQ = 27000000,    // Tang Nano 9K varsayılan frekansı (27 MHz)
    parameter BAUD_RATE = 115200      // Standart haberleşme hızı
)(
    input clk,
    input rstn,
    input rx_pin,           // PC'den gelen fiziksel kablo
    output reg [7:0] data,  // Alınan 8-bit veri
    output reg valid,       // "Yeni veri geldi" bayrağı
    input ack               // İşlemciden gelen "Veriyi okudum, bayrağı indir" sinyali
);

    localparam BIT_TMR_MAX = CLK_FREQ / BAUD_RATE;
    localparam BIT_TMR_HALF = BIT_TMR_MAX / 2;

    reg [2:0] state;
    reg [15:0] bit_timer;
    reg [2:0] bit_idx;
    reg [7:0] shift_reg;

    localparam IDLE = 0, START = 1, DATA = 2, STOP = 3;

    always @(posedge clk or negedge rstn) begin
        if (!rstn) begin
            state <= IDLE;
            valid <= 0;
            data <= 0;
        end else begin
            if (ack) valid <= 0; // İşlemci veriyi okudu, valid'i sıfırla
            
            case (state)
                IDLE: begin
                    bit_timer <= 0;
                    if (rx_pin == 0) state <= START; // Start biti algılandı
                end
                START: begin
                    if (bit_timer == BIT_TMR_HALF) begin
                        state <= DATA;
                        bit_timer <= 0;
                        bit_idx <= 0;
                    end else bit_timer <= bit_timer + 1;
                end
                DATA: begin
                    if (bit_timer == BIT_TMR_MAX) begin
                        bit_timer <= 0;
                        shift_reg[bit_idx] <= rx_pin;
                        bit_idx <= bit_idx + 1;
                        if (bit_idx == 7) state <= STOP;
                    end else bit_timer <= bit_timer + 1;
                end
                STOP: begin
                    if (bit_timer == BIT_TMR_MAX) begin
                        data <= shift_reg;
                        valid <= 1; // İşlemciye verinin hazır olduğunu bildir
                        state <= IDLE;
                    end else bit_timer <= bit_timer + 1;
                end
            endcase
        end
    end
endmodule