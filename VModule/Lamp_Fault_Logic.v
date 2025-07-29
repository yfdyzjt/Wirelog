module Lamp_Fault_Logic (
    input wire clk,            
    input wire in,
    output reg out
);

    always @(posedge clk) begin
        out <= in; 
    end

endmodule