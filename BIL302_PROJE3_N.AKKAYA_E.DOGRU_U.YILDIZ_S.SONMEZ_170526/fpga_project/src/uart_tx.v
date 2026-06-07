module uart_tx (
    input clk,
    input rstn,
    input valid,
    input [7:0] data,
    output reg tx_pin,
    output reg ready
);
    parameter CLKS_PER_BIT = 234; // 27 MHz / 115200 Baudrate

    reg [3:0] state;
    reg [15:0] clk_count;
    reg [7:0] tx_data;
    reg [2:0] bit_index;

    always @(posedge clk or negedge rstn) begin
        if (!rstn) begin
            state <= 0;
            tx_pin <= 1;
            ready <= 1;
            clk_count <= 0;
        end else begin
            case (state)
                0: begin // IDLE (Bekleme)
                    tx_pin <= 1;
                    ready <= 1;
                    if (valid) begin
                        tx_data <= data;
                        state <= 1;
                        ready <= 0;
                        clk_count <= 0;
                    end
                end
                1: begin // START BIT (Başlangıç)
                    tx_pin <= 0;
                    if (clk_count < CLKS_PER_BIT - 1) clk_count <= clk_count + 1;
                    else begin clk_count <= 0; state <= 2; bit_index <= 0; end
                end
                2: begin // DATA BITS (8-bit Veri)
                    tx_pin <= tx_data[bit_index];
                    if (clk_count < CLKS_PER_BIT - 1) clk_count <= clk_count + 1;
                    else begin
                        clk_count <= 0;
                        if (bit_index < 7) bit_index <= bit_index + 1;
                        else state <= 3;
                    end
                end
                3: begin // STOP BIT (Bitiş)
                    tx_pin <= 1;
                    if (clk_count < CLKS_PER_BIT - 1) clk_count <= clk_count + 1;
                    else begin clk_count <= 0; state <= 0; end
                end
            endcase
        end
    end
endmodule