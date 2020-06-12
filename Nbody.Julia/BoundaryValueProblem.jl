using DifferentialEquations; using Reduce;
using Plots;
using NLsolve;
using LinearAlgebra;

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
function bc1!(residual, u, p, t)
    len = 3
    dt = range(0.0, t[end], length=len);
    loss2 = [(p[j](i) - u(i)[j - 1])/len for i=dt, j=2:4] |> sum
    loss1 = [(p[j](i) - u(i)[j - 1])/len for i=dt, j=5:7] |> sum
    display([loss1, loss2])
    residual[1] = 0.0
    residual[2] = 0.0
    residual[3] = loss1
    residual[4] = 0.0
    residual[5] = loss2
    residual[6] = 0.0
end
function bc2!(residual, u, p, t)
    # len = 13
    #
    # dt = range(0.0, t[end], length=len);
    # loss1 = [(p[j](i) - u(i)[j - 1])/len for i=dt, j=2:2] |> sum
    # loss2 = [(p[j](i) - u(i)[j - 1])/len for i=dt, j=3:3] |> sum
    # loss3 = [(p[j](i) - u(i)[j - 1])/len for i=dt, j=6:6] |> sum
    # display([loss1, loss2, loss3])
    te = t[end];
    residual[1] = p[2](te) - u[end][1]
    residual[2] = 0.0
    residual[3] = p[4](te) - u[end][3]
    residual[4] = 0.0
    residual[5] = p[6](te) - u[end][5]
    residual[6] = 0.0
end
function bc3!(residual, u, p, t)
    residual[1:6] .= 0.0;
    residual[5] = u[1][5] + u[end][5];
    residual[1] = abs(u[end][4]) + abs(u[1][4]);
    residual[3] = abs(u[end][6]) + abs(u[1][6]);
end
function bc4!(residual, u, p, t)
    residual[1:6] .= 0.0;
    residual[1] = u(0.0)[1] - p[2](0.0) + u[end][1] - u[1][1];
    residual[2] = u(0.0)[2] + u[1][2] + u[end][2];
    residual[3] = u[end][3] - u[1][3];
    residual[4] = u[1][4] - u[end][4] + u(0.0)[4];
    residual[5] = u[1][5] - 2 * u[end][5] - u(0.0)[6];
    residual[6] = u[1][6] - u[end][6] + u(0.0)[6];

end
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
function Body3Aprox(Ay, Az, k, ω, l)
    x = t -> (-k * Ay * cos(ω*t)) + l
    y = t -> Ay * sin(ω*t)
    z = t -> Az * cos(ω*t)
    xd = t -> (k * Ay * ω * sin(ω*t))
    yd = t -> Ay * ω * cos(ω*t)
    zd = t -> - Az * ω * sin(ω*t)
    return [x, y, z, xd, yd, zd, abs(ω)];
end
function Lessi(Ay, Az, lambda, phi, k, nu, thata)
    return [
        t -> -k * Ay * cos(t*lambda + phi),
        t -> Ay * sin(t * lambda +phi),
        t -> Az * sin(t * nu + thata)
    ];
end
function ap(p, l, μ)
    x0 = p;
    diff = l;
    return (X) -> begin
        a = (F, x) -> begin
            k, Ay, ω, yd0, Az  = x
            F[1] = -k * Ay - (x0 - diff)
            F[2] = ω * Ay - yd0
            F[3] = 0.0
            F[4] = J([x0, 0.0, Az], [0.0, yd0, 0.0], μ) - J([ -k*(x0 - diff)*cos(ω*π) + diff, 0.0, Az*cos(ω*π)], [0.0, ω*cos(ω*π), 0.0], μ)
            F[5] = 0.0
        end
        return nlsolve(a, X).zero;
    end
end
function testInit(u0, t)
    ode = ODEProblem(Body3!, u0, (-t, t), [1.0/82.2800178])
    sol2 = solve(ode)
    return sol2;
end
x0 = initxz = 1.13
function wholeSolve(x0, k, Ay, ω, ydo, z0)
    l2 = 1.1556821603
    μ = 1.0/82.2800178
    k, Ay, ω, yd0, z0 = ap(initxz, x0, μ)([k, Ay, ω, ydo, z0])
    display(["k" => k, "Ay" => Ay, "ω" => ω, "ẏ0" => yd0, "zo" => z0]);

    initial = [x0, 0.0, z0, 0.0, yd0, 0.0]
    aprox = Body3Aprox(Ay, z0, k, ω, l2)
    tspan = (-Float64(π/ω), Float64(π/ω))
    display(π/ω)

    bvp1 = BVProblem(Body3!, bc4!, initial, tspan, [μ, aprox...])
    sol1 = solve(bvp1, Shooting(Vern7()), precision=10e-12)
    # for i=1:10
    #     initial = sol1(0.0);
    #     display(initial);
    #     bvp1 = BVProblem(Body3!, bc4!, initial, tspan, [μ, aprox...])
    #     sol1 = solve(bvp1, Shooting(Vern7()), precision=10e-12)
    # end
    # solv2 = solve(bvp1, Shooting(GeneralMIRK4()), dt=0.00001)
    # plot(sol1, vars=(1, 2, 3))
    # plot(t -> aprox[1](t), t -> aprox[3](t), t -> aprox[3](t), 0.0:0.001:10)
    # plot(t -> aprox[2](t), 0:0.01:3.14/ω)
    ode = ODEProblem(Body3!, sol1[1], (-π/ω*10, π/ω*10), [μ])
    sol2 = solve(ode)
    return [sol2, sol1];
end
# s1, s2 = wholeSolve(1.08, .6, 0.11, 13.6, 0.0004, -0.11)
# plot(s1, vars=(1, 2, 3))
# s1.u[1]
# s2(-0.001)
