using System;
using System.Collections.Generic;

abstract class Expr
{
    public abstract override string ToString();
    public abstract int Eval(Dictionary<string, int> env);
}

class CstI : Expr
{
    public readonly int Value;

    public CstI(int value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public override int Eval(Dictionary<string, int> env)
    {
        return Value;
    }
    
}

class Var : Expr
{
    public readonly string Name;

    public Var(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }

    public override int Eval(Dictionary<string, int> env)
    {
        if (env.TryGetValue(Name, out int value))
        {
            return value;
        }

        throw new Exception("Variable does not exist in env");
    }
}

abstract class Binop : Expr
{
    public readonly Expr E1;
    public readonly Expr E2;

    public Binop(Expr e1, Expr e2)
    {
        E1 = e1;
        E2 = e2;
    }
}

class Add : Binop
{
    public Add(Expr e1, Expr e2) : base(e1,e2) { }

    public override string ToString()
    {
        return $"({E1} + {E2})";
    }

    public override int Eval(Dictionary<string, int> env)
    {
        return (E1.Eval(env) + E2.Eval(env));
    }
    
}

class Mul : Binop
{
    public Mul(Expr e1, Expr e2) : base(e1,e2) {}

    public override string ToString()
    {
        return $"({E1} * {E2})";
    }

    public override int Eval(Dictionary<string, int> env)
    {
        return (E1.Eval(env) * E2.Eval(env));
    }
}

class Sub : Binop
{
    public Sub(Expr e1, Expr e2) : base(e1,e2) {}

    public override string ToString()
    {
        return $"({E1} - {E2})";
    }

    public override int Eval(Dictionary<string, int> env)
    {
        return (E1.Eval(env) - E2.Eval(env));
    }
}


class Machine
{
    static void Main()
    {
        Expr e = new Add(new CstI(17), new Var("z"));
        // Three more expressions in abstract syntax :O
        Expr e1 = new Mul(new CstI(1), new Var("en"));
        Expr e2 = new Sub(new CstI(2), new Var("to"));
        Expr e3 = new Add(new CstI(3), new Var("tre"));

        List<Expr> explist = new List<Expr> { e, e1, e2, e3 };

        foreach (Expr xp in explist)
        {
            Console.Out.WriteLine(xp.ToString());
        }
    }
}

