using System;
using System.Collections.Generic;

abstract class Expr
{
    public abstract override string ToString();
    public abstract int Eval(Dictionary<string, int> env);
    public abstract Expr Simplify();
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

    public override Expr Simplify()
    {
        return this;
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
    
    public override Expr Simplify()
    {
        return this;
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
    
    public override Expr Simplify()
    {
        Expr s1 = E1.Simplify();
        Expr s2 = E2.Simplify();
        
        switch (s1,s2)
        {
            case (CstI{Value:0},var to):
                return to;
            case (var en, CstI{Value:0}):
                return en;
            default:
                return new Add(s1, s2);
        }
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
    
    public override Expr Simplify()
    {
        Expr s1 = E1.Simplify();
        Expr s2 = E2.Simplify();
        
        switch (s1,s2)
        {
            case (CstI{Value:0},var to):
                return new CstI(0);
            case (var en, CstI{Value:0}):
                return new CstI(0);
            case (CstI{Value:1},var to):
                return to;
            case (var en, CstI{Value:1}):
                return en;
            default:
                return new Mul(s1, s2);
        }
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
    
    public override Expr Simplify()
    {
        Expr s1 = E1.Simplify();
        Expr s2 = E2.Simplify();
        
        // Havent found a good way to compare the two in e1 = e2 case returning 0
        // Couldnt fint a good comparer for classes. 
        switch (s1,s2)
        {
            case (var en, CstI{Value:0}):
                return en;
            case (var en, var to) when en.ToString() == to.ToString():
                return (new CstI(0));
            default:
                return new Sub(s1, s2);
        }
    }
}


class Program
{
    static void Main()
    {
        Expr e = new Add(new CstI(17), new Var("z"));
        // Three more expressions in abstract syntax :O
        Expr e1 = new Mul(new CstI(1), new Var("en"));
        Expr e2 = new Sub(new CstI(2), new Var("to"));
        Expr e3 = new Add(new CstI(3), new Var("tre"));
        Expr e4 = new Sub(new CstI(2), new CstI(2));
        Expr e5 = new Sub(new CstI(2), new CstI(0));
        Expr e6 = e4.Simplify();
        Expr e7 = e5.Simplify();

        List<Expr> explist = new List<Expr> { e, e1, e2, e3, e4, e5,e6,e7};

        foreach (Expr xp in explist)
        {
            Console.Out.WriteLine(xp.ToString());
        }
    }
}

