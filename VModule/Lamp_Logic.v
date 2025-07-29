module Lamp_Logic #(
    parameter INIT_VALUE = 0  
)(
    input wire clk,            
    input wire reset,         
    input wire in,
    output reg out
);

    initial begin
        out = INIT_VALUE;
    end

    always @(posedge clk) begin
        if (reset) begin
            out <= INIT_VALUE;
        end else begin
            out <= in ^ out; 
        end
    end

endmodule