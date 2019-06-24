namespace KKdMainLib.Types
{
    public interface KeyFrame<TKey, TVal>
    {
        KeyFrame<TKey, TVal> Check();
        KeyFrame<TKey, TVal> ToKeyFrameT0();
        KeyFrame<TKey, TVal> ToKeyFrameT1();
        KeyFrame<TKey, TVal> ToKeyFrameT2();
        KeyFrame<TKey, TVal> ToKeyFrameT3();
        string ToString();
        string ToString(bool Brackets);
    }
    
    public struct KeyFrameT0<TKey, TVal> : KeyFrame<TKey, TVal>
    {
        public TKey Frame;

        public KeyFrame<TKey, TVal> ToKeyFrameT0() =>
            new KeyFrameT0<TKey, TVal> { Frame = Frame };
        public KeyFrame<TKey, TVal> ToKeyFrameT1() =>
            new KeyFrameT1<TKey, TVal> { Frame = Frame };
        public KeyFrame<TKey, TVal> ToKeyFrameT2() =>
            new KeyFrameT2<TKey, TVal> { Frame = Frame };
        public KeyFrame<TKey, TVal> ToKeyFrameT3() =>
            new KeyFrameT3<TKey, TVal> { Frame = Frame };

        public KeyFrame<TKey, TVal> Check() => this;

        public override string ToString() =>
            Main.ToString(Frame);
        public string ToString(bool Brackets) =>
            Main.ToString(Frame);
    }

    public struct KeyFrameT1<TKey, TVal> : KeyFrame<TKey, TVal>
    {
        public TKey Frame;
        public TVal Value;

        public KeyFrame<TKey, TVal> ToKeyFrameT0() =>
            new KeyFrameT0<TKey, TVal> { Frame = Frame };
        public KeyFrame<TKey, TVal> ToKeyFrameT1() =>
            new KeyFrameT1<TKey, TVal> { Frame = Frame, Value = Value };
        public KeyFrame<TKey, TVal> ToKeyFrameT2() =>
            new KeyFrameT2<TKey, TVal> { Frame = Frame, Value = Value };
        public KeyFrame<TKey, TVal> ToKeyFrameT3() =>
            new KeyFrameT3<TKey, TVal> { Frame = Frame, Value = Value };

        public KeyFrame<TKey, TVal> Check()
        {
            if (Value.Equals(default(TVal))) return ToKeyFrameT0();
            return this;
        }

        public override string ToString() => ToString(true);
        public string ToString(bool Brackets) =>
            (Brackets ? "(" : "") + Main.ToString(Frame) + "," +
                Main.ToString(Value) + (Brackets ? ")" : "");
    }
    
    public struct KeyFrameT2<TKey, TVal> : KeyFrame<TKey, TVal>
    {
        public TKey Frame;
        public TVal Value;
        public TVal Interpolation;

        public KeyFrame<TKey, TVal> ToKeyFrameT0() =>
            new KeyFrameT0<TKey, TVal> { Frame = Frame };
        public KeyFrame<TKey, TVal> ToKeyFrameT1() =>
            new KeyFrameT1<TKey, TVal> { Frame = Frame, Value = Value };
        public KeyFrame<TKey, TVal> ToKeyFrameT2() =>
            new KeyFrameT2<TKey, TVal> { Frame = Frame, Value = Value,
                Interpolation  = Interpolation};
        public KeyFrame<TKey, TVal> ToKeyFrameT3() =>
            new KeyFrameT3<TKey, TVal> { Frame = Frame, Value = Value,
                Interpolation1 = Interpolation, Interpolation2 = Interpolation };

        public KeyFrame<TKey, TVal> ToKeyFrameT3(KeyFrame<TKey, TVal> Previous) =>
            Previous is KeyFrameT2<TKey, TVal> PreviousT2 ? 
            new KeyFrameT3<TKey, TVal> { Frame = Frame, Value = Value,
                Interpolation1 = PreviousT2.Interpolation, Interpolation2 = Interpolation } :
            new KeyFrameT3<TKey, TVal> { Frame = Frame, Value = Value,
                Interpolation1 =            Interpolation, Interpolation2 = Interpolation };

        public KeyFrame<TKey, TVal> Check()
        {
                 if (Value.Equals(default(TVal)) && Interpolation.Equals(default(TVal)))
                                                  return ToKeyFrameT0();
            else if (Value.Equals(default(TVal))) return ToKeyFrameT1();
            return this;
        }

        public override string ToString() => ToString(true);
        public string ToString(bool Brackets) =>
            (Brackets ? "(" : "") + Main.ToString(Frame) + "," + Main.
            ToString(Value) + "," + Main.ToString(Interpolation) + (Brackets ? ")" : "");
    }

    public struct KeyFrameT3<TKey, TVal> : KeyFrame<TKey, TVal>
    {
        public TKey Frame;
        public TVal Value;
        public TVal Interpolation1;
        public TVal Interpolation2;

        public KeyFrame<TKey, TVal> ToKeyFrameT0() =>
            new KeyFrameT0<TKey, TVal> { Frame = Frame };
        public KeyFrame<TKey, TVal> ToKeyFrameT1() =>
            new KeyFrameT1<TKey, TVal> { Frame = Frame, Value = Value };
        public KeyFrame<TKey, TVal> ToKeyFrameT2() =>
            new KeyFrameT2<TKey, TVal> { Frame = Frame, Value = Value, Interpolation = Interpolation1 };
        public KeyFrame<TKey, TVal> ToKeyFrameT3() =>
            new KeyFrameT3<TKey, TVal> { Frame = Frame, Value = Value,
                Interpolation1 = Interpolation1, Interpolation2 = Interpolation2 };

        public KeyFrame<TKey, TVal> Check()
        {
                 if (Value.Equals(default(TVal)) && Interpolation1.Equals(default(TVal)) &&
            Interpolation2.Equals(default(TVal))) return ToKeyFrameT0();
            else if (Interpolation1.Equals(default(TVal)) &&
            Interpolation2.Equals(default(TVal))) return ToKeyFrameT1();
            else if (Interpolation1.Equals(Interpolation2))
                                                  return ToKeyFrameT2();
            return this;
        }

        public override string ToString() => ToString(true);
        public string ToString(bool Brackets) =>
            (Brackets ? "(" : "") + Main.ToString(Frame) + "," + Main.ToString(Value) + "," +
            Main.ToString(Interpolation1) + "," + Main.ToString(Interpolation2) + (Brackets ? ")" : "");

        public KeyFrame<TKey, TVal> ToKeyFrameT2(KeyFrame<TKey, TVal>
            Previous, out KeyFrameT2<TKey, TVal> Current)
        {
            if (Previous is KeyFrameT2<TKey, TVal> PreviousT2)
                Current = new KeyFrameT2<TKey, TVal> { Frame = PreviousT2.Frame,
                    Value = PreviousT2.Value, Interpolation = Interpolation1 };
            else
                Current = new KeyFrameT2<TKey, TVal> { Frame = Frame,
                    Value = Value, Interpolation = Interpolation1 };
            return new KeyFrameT2<TKey, TVal> { Frame = Frame,
                Value = Value, Interpolation = Interpolation2 };
        }
        public KeyFrame<TKey, TVal> ToKeyFrameT2(KeyFrame<TKey, TVal>
            Previous, out KeyFrame<TKey, TVal> Current)
        {
            if (Previous is KeyFrameT2<TKey, TVal> PreviousT2)
                Current = new KeyFrameT2<TKey, TVal> { Frame = PreviousT2.Frame,
                    Value = PreviousT2.Value, Interpolation = Interpolation1 };
            else
                Current = new KeyFrameT2<TKey, TVal> { Frame = Frame,
                    Value = Value, Interpolation = Interpolation1 };
            return new KeyFrameT2<TKey, TVal> { Frame = Frame,
                Value = Value, Interpolation = Interpolation2 };
        }
    }
}
