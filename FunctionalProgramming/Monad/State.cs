﻿using System;
using FunctionalProgramming.Basics;

namespace FunctionalProgramming.Monad
{
    public class State<TState, TValue>
    {
        private readonly Func<TState, Tuple<TState, TValue>> _run;
 
        public State(Func<TState, Tuple<TState, TValue>> run)
        {
            _run = run;
        }

        public TValue Eval(TState s)
        {
            return _run(s).Item2;
        }

        public Tuple<TState, TValue> Run(TState s)
        {
            return _run(s);
        }

        public State<TState, TResult> FMap<TResult>(Func<TValue, TResult> f)
        {
            return new State<TState, TResult>(s =>
            {
                var result = _run(s);
                return Tuple.Create(result.Item1, f(result.Item2));
            });
        }

        public State<TState, TResult> Bind<TResult>(Func<TValue, State<TState, TResult>> f)
        {
            return new State<TState, TResult>(s =>
            {
                var result = _run(s);
                return f(result.Item2)._run(result.Item1);
            });
        }
    }

    public static class StateExtensions
    {
        public static State<TState, TValue> Insert<TState, TValue>(this TValue t)
        {
            return new State<TState, TValue>(s => Tuple.Create(s, t));
        }

        public static State<TState, TValue> Get<TState, TValue>(this Func<TState, TValue> f)
        {
            return new State<TState, TValue>(s => Tuple.Create(s, f(s)));    
        }

        public static State<TState, Unit> Mod<TState>(this Func<TState, TState> f)
        {
            return new State<TState, Unit>(s => Tuple.Create(f(s), Unit.Only));
        }

        public static State<TState, TResult> Select<TState, TValue, TResult>(this State<TState, TValue> m,
            Func<TValue, TResult> f)
        {
            return m.FMap(f);
        }

        public static State<TState, TResult> SelectMany<TState, TValue, TResult>(this State<TState, TValue> m,
            Func<TValue, State<TState, TResult>> f)
        {
            return m.Bind(f);
        }

        public static State<TState, TSelect> SelectMany<TState, TValue, TResult, TSelect>(this State<TState, TValue> m,
            Func<TValue, State<TState, TResult>> f, Func<TValue, TResult, TSelect> selector)
        {
            return m.Bind(a => f(a).Bind(b => new State<TState, TSelect>(s => Tuple.Create(s, selector(a, b)))));
        }
    }
}
