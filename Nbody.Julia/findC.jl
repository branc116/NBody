using DifferentialEquations;
using Plots;

function AnaBody3!(du,u,p,t)
    x1, x, y1, y, z1, z = u;
    ẋ1, ẋ, ẏ1, ẏ, ż1, ż = du;
    c = p[1]
    du[1] = 2 * ẏ + (1 + 2*c)*x;
    du[2] = x1;
    du[3] = -2 * ẋ -(c - 1)y
    du[4] = y1;
    du[5] = -c * z;
    du[6] = z1;
end
change = (x::Float64) -> 1 - atan(x^3/100000000) / π * 2 / 100.0

function findC(c::Float64, x0::Float64, yd0::Float64, tend::Float64)
    ẋ, x, ẏ, y, ż, z = u0 = [0.0, -0.01, 0.000001, 0.0, 0.0, 0.0]
    tspan = (0.0,tend)
    ok = false
    while(!ok)
        ok = true;
        p = [ c ]#Float64(1 / 82.2800178)]
        prob = ODEProblem(AnaBody3!,u0,tspan,p)
        solution = solve(prob)
        if (abs(solution[end][1]) > 10.0)
            c *= change(solution[end][1]);
            ok = false;
        end
        display(("c" => c,"xt" => solution[end][2], "yt" => solution[end][4], "xdt" => solution[end][1]))
        if (ok)
            return solution;
        end
    end
end

plot(findC(0.55587835788838723, -0.01, 1.0, Float64(π)*5), vars=(2, 4, 6))