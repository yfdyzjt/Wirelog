module Gate_Logic (        
    input wire clk,
    input wire logic_reset,       
    input wire in, 
    output wire out
);

    reg has_out;
    reg prev_in;

    initial begin
        has_out = 0;
        prev_in = in;
    end

    assign out = has_out ? 0 : (prev_in ^ in);

    always @(posedge clk) begin
        prev_in <= in;
        if (logic_reset) begin
            has_out <= 0;
        end
        else if (out) begin
            has_out <= 1;
        end
    end

endmodule