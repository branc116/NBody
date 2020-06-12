using DifferentialEquations; using Plots; using WebIO; using Interact;
using LinearAlgebra;
using Reduce;
using NLsolve;

ẍ = :(2*ẏ + x - (1 - μ) * (x+μ) / ((x+μ)^2 + y^2 +z^2)^(3/2) - μ * (x-1+μ) / ((x-1+μ)^2 + y^2 + z^2)^(3/2))
ÿ = :(-2*ẋ + y - (1 - μ) * (y) / ((x+μ)^2 + y^2 +z^2)^(3/2) - μ * y / ((x-1+μ)^2 + y^2 + z^2)^(3/2))
z̈ = :( -(1 - μ) * z / ((x+μ)^2 + y^2 +z^2)^(3/2) - μ * z / ((x-1+μ)^2 + y^2 + z^2)^(3/2))
A =[Algebra.df(j, i) for j=[:ẋ :ẏ :ż ẍ ÿ z̈], i = [:x :y :z :ẋ :ẏ :ż]] |> x -> reshape(x, 6, 6)
B = A |> x -> reshape(x, 36) .|> (x -> Expr(:function, :((μ, x, y, z, ẋ, ẏ, ż)), x)) .|> eval |> x -> reshape(x, 6, 6)

U(x, μ) = 0.5 * (x[1]^2 + x[2]^2) + (1 - μ) / norm(x + [μ, 0, 0]) + μ / norm(x + [μ - 1, 0, 0])
J(x, ẋ, μ) = 2 * U(x, μ) - norm(ẋ)^2
Bline = reshape(B, length(B));

function F(X, μ)
    return Bline .|> (x -> x(μ, X[1], X[2], X[3], X[4], X[5], X[6])) |> x -> reshape(x, 6, 6)
end
function Xdotdot(X, μ)
    x, y, z, ẋ, ẏ, ż = X
    ẍ = 2*ẏ + x - (1 - μ) * (x+μ) / (( (x+μ)^2 + y^2 +z^2 )^(3/2)) - μ * (x-1+μ) / (( (x-1+μ)^2 + y^2 + z^2 )^(3/2) )
    ÿ = -2*ẋ + y - (1 - μ) * (y) / (( (x+μ)^2 + y^2 +z^2 )^(3/2)) - μ * y / (( (x-1+μ)^2 + y^2 + z^2 )^(3/2) );
    z̈ = - (1 - μ) * z / (( (x+μ)^2 + y^2 +z^2 )^(3/2)) - μ * z / (( (x-1+μ)^2 + y^2 + z^2 )^(3/2) );
    return [ẍ, ÿ, z̈]
end
function Body33!(du,u,p,t)
    x, y, z, x1, y1, z1 = u;
    P = reshape(u[7:end], 6, 6)
    ẋ, ẏ, ż, ẋ1, ẏ1, ż1, Ṗ = du;
    μ = p[1]
    du[1] = x1;
    du[2] = y1;
    du[3] = z1;
    du[4] = 2*ẏ + x - (1 - μ) * (x+μ) / (( (x+μ)^2 + y^2 +z^2 )^(3/2)) - μ * (x-1+μ) / (( (x-1+μ)^2 + y^2 + z^2 )^(3/2) );
    du[5] = -2*ẋ + y - (1 - μ) * (y) / (( (x+μ)^2 + y^2 +z^2 )^(3/2)) - μ * y / (( (x-1+μ)^2 + y^2 + z^2 )^(3/2) );
    du[6] = - (1 - μ) * z / (( (x+μ)^2 + y^2 +z^2 )^(3/2)) - μ * z / (( (x-1+μ)^2 + y^2 + z^2 )^(3/2) );
    du[7:end] = reshape(F(u, μ) * P, 36)
end
function Body3!(du,u,p,t)
    x, y, z, x1, y1, z1 = u;
    ẋ, ẏ, ż, ẋ1, ẏ1, ż1 = du;
    μ = p[1]
    du[1] = x1;
    du[2] = y1;
    du[3] = z1;
    du[4] = 2*ẏ + x - (1 - μ) * (x+μ) / (( (x+μ)^2 + y^2 +z^2 )^(3/2)) - μ * (x-1+μ) / (( (x-1+μ)^2 + y^2 + z^2 )^(3/2) );
    du[5] = -2*ẋ + y - (1 - μ) * (y) / (( (x+μ)^2 + y^2 +z^2 )^(3/2)) - μ * y / (( (x-1+μ)^2 + y^2 + z^2 )^(3/2) );
    du[6] = - (1 - μ) * z / (( (x+μ)^2 + y^2 +z^2 )^(3/2)) - μ * z / (( (x-1+μ)^2 + y^2 + z^2 )^(3/2) );
end
function ap(p, l, μ)
    x0, z0 = p;
    diff = l;

    return (F, x) -> begin
        k, Ay, ω, yd0  = x
        F[1] = -k * Ay - (x0 - diff)
        F[2] = ω * Ay - yd0
        F[3] = Ay - z0
        F[4] = J([x0, 0.0, z0], [0.0, yd0, 0.0], μ) - J([ -k*(x0 - diff)*cos(ω*π) + diff, 0.0, Ay*cos(z0*π)], [0.0, ω*cos(ω*π), 0.0], μ)
    end
end
function zavrsni(u0, p)
    funcs = Body3Aprox(u0, p[1])
    period = funcs[7];
    tspan = (0.0, pi/period)
    # p = [0.012277471]
    ucopy = [i for i=u0]
    wanted = funcs[1:6] .|> i -> i(π/period);
    wanted0 = funcs[1:6] .|> i -> i(0.0);
    display([J(u0[1:3], u0[4:6], p[1]), π/period]);
    Jacobi = (J, X) -> begin
        F = Bline .|> (x -> x(p[1], X[1], X[2], X[3], X[4], X[5], X[6])) |> x -> reshape(x, 6, 6)
        J[1:6] = F[1:6]
    end
    notLinear = (F, x) -> begin
        ucopy[3] = x[3];
        ucopy[5] = x[5];
        prob = ODEProblem(Body3!, ucopy, tspan, p);
        solution = solve(prob, precision=10e-17, dt=10e-16, verbose=false);
        F[1:6] = (wanted - solution[end][1:6]) .* [1.0, 1.0, 1.0, 1.0, 1.0, 1];
        # F[7:12] = (wanted0 - x[1:6]) .* [1, 1, 0, 1, 0, 1];
    end
    # nSol = nlsolve(notLinear, [u0..., wanted0...], iterations= 10000);
    nSol = nlsolve(notLinear, u0, iterations= 10000);
    prob = ODEProblem(Body3!, nSol.zero[1:6], 3*π/period, p);
    solution = solve(prob, precision=10e-17, dt=10e-16, verbose=false);
    fig = plot(solution, vars=(0, 1));
    # plot!(fig, solution, vars=(0, 2));
    # plot!(fig, solution, vars=(0, 3));
    plot!(fig, t -> t, t -> (1-p[1]), 0:0.1:(3*π));
    display(fig);
    return [nSol, solution];
end
a = zavrsni([1.1101, 0.0, 0.131, 0.0, -0.9, 0.0], [1/82.2800178])

function Body3Aprox(u, μ)
    l2 = 1.1556821603;
    Ay = u[3];
    ω = u[5] / Ay;
    xdiff = l2
    k = -(u[1] - xdiff ) / Ay;
    x = t -> (-k * Ay * cos(ω*t)) + xdiff
    y = t -> Ay * sin(ω*t)
    z = t -> Ay * cos(ω*t)
    xd = t -> (k * Ay * ω * sin(ω*t))
    yd = t -> Ay * ω * cos(ω*t)
    zd = t -> - Ay * ω * sin(ω*t)
    return [x, y, z, xd, yd, zd, abs(ω)];
end
function minY(sol, interval)
    curMin = (10e20, 10e20)
    for i=interval
        if (abs(sol(i)[2]) < curMin[2])
            curMin = (i, sol(i)[2])
        end
    end
    return curMin;
end
function testSolution(solution)
    if(solution.retcode == :success)
        if (minimum(i -> i[1], solution.u) > 1 && maximum(i -> i[1], solution.u) < 3 &&
            minimum(i -> i[2]), solution.u > -1 && maximum(i -> i[2], solution.u) < 1 &&
            minimum(i -> i[3]), solution.u > -1 && maximum(i -> i[3], solution.u) < 1 &&
            minimum(i -> i[4]), solution.u > -1 && maximum(i -> i[4], solution.u) < 1 &&
            minimum(i -> i[5]), solution.u > -1 && maximum(i -> i[5], solution.u) < 1 &&
            maximum(i -> i[1], solution.u) - minimum(i -> i[1], solution.u) > 0.001 &&
            maximum(i -> i[2], solution.u) - minimum(i -> i[2], solution.u) > 0.001 &&
            maximum(i -> i[4], solution.u) - minimum(i -> i[4], solution.u) > 0.001)
            return true;
        end
    end
    return false;
end

function solveInitial(u0, Tt, side, n)
    x, y, z, x1, y1, z1 = u0
    tspan = (0.0, Tt)
    p = [0.012277471]
    latY0 = 0;
    # display(u0[1:6]);
    for k=1:n
        TtOld = Tt
        prob = ODEProblem(Body33!,u0,tspan,p)
        solution = solve(prob, precision=10e-17, dt=10e-16, verbose=false)
        # display(solution);
        try
            # Tt = minY(solution, 0.004:0.00001:Tt)[1]
            a = [solution(Tt)[4:6]..., Xdotdot(solution(Tt)[1:6], 1.0 / 82.2800178)...]
            endd = solution[end]
            display([endd[2], endd[4], endd[6]]);
            phi = reshape(solution(Tt)[7:end], 6, 6);
            l = reshape(([[(phi[4, 3] - phi[2, 3] * a[4] / a[2]), (phi[4, 5] - phi[2, 5]*a[4]/a[2])]
                        [(phi[6, 3] - phi[2, 3] * a[6] / a[2]), (phi[6, 5] - phi[2, 5]*a[6]/a[2])]]), 2, 2);
            # display(l);
            r = [-a[1], -a[3]]

            bump = transpose(l)^-1*r;
            if (norm(bump) < 10e-15)
                break;
            end
            if (norm(bump) > 10e2)
                break;
            end
            x, y, z, x1, y1, z1 = u0 = [x, 0.0, z + bump[1] * side[1], 0.0, y1 + bump[2] * side[2], 0.0, reshape(Matrix{Float64}(I, 6, 6), 36)...];
            # display(bump)
            Tt = TtOld;
        catch
            return solution
        end
    end
    # display(u0[1:6]);
    tspan = (0.0, Tt)
    prob = ODEProblem(Body33! , u0, tspan, p)
    solution = solve(prob, precision=10e-13, dt=10e-12)
    return solution;
end
function solveInitial(u0, side, steps)
    time = Body3Aprox(u0, 1/82.2800178)[end] / 2
    j = J(u0[1:3], u0[4:6], 1/82.2800178)
    if (j < 3)
        display("J small")
        display(j)
    end
        return;
    if (j > 4)
        display("J large")
        display(j)
        return;
    end
    display([time, j])
    return solveInitial(u0, time, side, steps);
end
# solveInitial = (u0, Tt) -> solveInitial(u0, Tt, [1, 1], 100)
# solveInitial = (u0, Tt, side) -> solveInitial(u0, Tt, side, 100)
using Base.Threads
safe = true;
function findAllSolutions()
    @threads for x0=1.001:0.01:2.0
        for yd0=0.001:0.01:1.5, z0=-0.2:0.1:0.2, t=0.01:0.01:0.5
            i = Matrix{Float64}(I, 6, 6)
            if (!safe)
                return;
            end
            try
            sol =solveInitial([x0, 0.0, z0, 0.0, yd0, 0.0, i...], t)
            if (testSolution(sol))
                display(sol)
            end
            catch
            end
            if (!safe)
                return;
            end
        end
    end
end
function Body3AproxYT(x0, z0, k, Ay, ω, yd0)
    l2 = 1.1556821603;
    return nlsolve(ap([x0, z0], l2, 1.0 / 82.2800178), [k, Ay, ω, yd0], iterations = 10000)
end
function solveInitial(x0, z0, k, Ay, ω, yd0, side, steps)
    l2 = 1.1556821603;
    nl = nlsolve(ap([x0, z0], l2, 1.0 / 82.2800178), [k, Ay, ω, yd0], iterations = 10000)
    k, Ay, ω, yd0 = nl.zero
    if (!nl.x_converged)
        return solveInitial([x0, 0.0, z0, 0.0, yd0, 0, Matrix{Float64}(I, 6, 6)...], ω*π, side, steps);
    end
    return nl;
end

plotly()
i = Matrix{Float64}(I, 6, 6);
ic = [1.00111, 0.0, 0.05, 0.0, 0.010019430719383188711, 0.0]

# sol = solveInitial([1.01, 0.0, 0.05, 0.0, 0.1, 0.0, Matrix{Float64}(I, 6, 6)... ], [.2, .2], 10);
sol = solveInitial([0.83946302646687, 0.0, 0.0, 0.0, -0.02596831282986, 0.0, Matrix{Float64}(I, 6, 6)... ], 2.69239959528586, [0.1, 0.1], 100)
sol = nl = solveInitial(1.10, -0.1, 0.1, 1.0, 0.01, 0.02, [0.1, 0.1], 10)
# solved = solveInitial([ic..., reshape(i, 36)...], [1.0, 1.0], 10)
# display(J(ic[1:3], ic[4:6], 1.0 / 82.2800178))
plot(sol, vars=(1, 2, 3))
plot(sol, vars=(0, 2))
plot(sol, vars=(0, 6))
plot(sol, vars=(0, 4))
# display(plot(solved, vars=(1, 2, 3)))
# minY(solved, 0.001:0.0001:0.35)
# gr()
# plot(solved, vars=(0, 1))
