module Output_Logic (
    input wire clk,            
    input wire logic_reset,         
    input wire in,
    output reg out
);

    initial begin
        out = 0;
    end

    always @(posedge clk) begin
        if (logic_reset) begin
            out <= 0;
        end else begin
            out <= in ^ out; 
        end
    end

endmodule