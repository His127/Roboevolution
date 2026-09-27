using System;

public struct BigNumber {

    public double Mantissa {get; private set;} // 1 <= |Mantissa| < 10
    public int Exponent {get; private set;}

    public BigNumber(double value){
        if(double.IsNaN(value) || double.IsInfinity(value)) {
            throw new ArgumentException("BigNumber cannot contain NaN or Infinity.");
        }

        if(value == 0) {
            Mantissa = 0;
            Exponent = 0;
            return;
        }

        Exponent = (int)Math.Floor(Math.Log10(Math.Abs(value)));
        Mantissa = value / Math.Pow(10, Exponent);
    }

    public BigNumber(double mantissa, int exponent){
        Mantissa = mantissa;
        Exponent = exponent;

        Normalize();
    }

    public static implicit operator BigNumber(double value){
        return new BigNumber(value);
    }

    public static explicit operator double(BigNumber value) {
        return value.Mantissa * Math.Pow(10, value.Exponent);
    }

    // Matissa zurück in den Wertebereich 1 <= |Mantissa| < 10 bringen
    private void Normalize(){
        if(Mantissa == 0){
            Exponent = 0;
            return;
        }

        int additionalExponent = (int)Math.Floor(Math.Log10(Math.Abs(Mantissa)));

        Mantissa /= Math.Pow(10, additionalExponent);
        Exponent += additionalExponent;
    }

    public override string ToString() {
        return $"{Mantissa}e{Exponent}";
    }

    // ============================= Grundlegende Rechenoperationen ===========================================

    // Zahl auf einen anderen Exponenten anpassen benötigt für Operatoren
    private double GetMantissa(int exponent) {
        return Mantissa * Math.Pow(10, Exponent - exponent);
    }

    // Addition
    public static BigNumber operator +(BigNumber a, BigNumber b){
        int exponent = Math.Max(a.Exponent, b.Exponent);
        
        double mantissaA = a.GetMantissa(exponent);
        double mantissaB = b.GetMantissa(exponent);

        return new(mantissaA + mantissaB, exponent);
    }
    
    // Subtraktion 
    public static BigNumber operator -(BigNumber a, BigNumber b){
        int exponent = Math.Max(a.Exponent, b.Exponent);

        double mantissaA = a.GetMantissa(exponent);
        double mantissaB = b.GetMantissa(exponent);

        return new(mantissaA - mantissaB, exponent);
    }

    // Negieren
    public static BigNumber operator -(BigNumber value) {
        return new(-value.Mantissa, value.Exponent);
    }

    // Multiplizieren
    public static BigNumber operator *(BigNumber a, BigNumber b) {
        double mantissa = a.Mantissa * b.Mantissa;
        int exponent = a.Exponent + b.Exponent;

        return new(mantissa, exponent);
    }

    // Dividieren
    public static BigNumber operator /(BigNumber a, BigNumber b) {
        if(b.Mantissa == 0) {
            throw new DivideByZeroException();
        }
        double mantissa = a.Mantissa / b.Mantissa;
        int exponent = a.Exponent - b.Exponent;

        return new(mantissa, exponent);
    }

    // ============================= Vergleichsoperationen ===========================================
    
    // Größer als
    public static bool operator >(BigNumber a, BigNumber b) {
        if (a.Mantissa >= 0 && b.Mantissa < 0) {
            return true;
        }else if (a.Mantissa < 0 && b.Mantissa >= 0) {
            return false;
        }

        if (a.Exponent != b.Exponent) {
            return a.Mantissa >= 0
                ? a.Exponent > b.Exponent
                : a.Exponent < b.Exponent;
        }

        return a.Mantissa > b.Mantissa;
    }

    // Kleiner als
    public static bool operator <(BigNumber a, BigNumber b) {
        if (a.Mantissa >= 0 && b.Mantissa < 0) {
            return false;
        }else if (a.Mantissa < 0 && b.Mantissa >= 0) {
            return true;
        }

        if (a.Exponent != b.Exponent) {
            return a.Mantissa >= 0
                ? a.Exponent < b.Exponent
                : a.Exponent > b.Exponent;
        }

        return a.Mantissa < b.Mantissa;
    }

    // Größer gleich
    public static bool operator >=(BigNumber a, BigNumber b) {
        return a > b || a == b;
    }

    // Kleiner gleich
    public static bool operator <=(BigNumber a, BigNumber b) {
        return a < b || a == b;
    }

    // Gleich
    public static bool operator ==(BigNumber a, BigNumber b) {
        return a.Mantissa == b.Mantissa && a.Exponent == b.Exponent;
    }

    // Ungleich
    public static bool operator !=(BigNumber a, BigNumber b) {
        return !(a == b);
    }

    // Objekt vergleich
    public override bool Equals(object obj) {
        return obj is BigNumber other && this == other;
    }

    // Objekt Hashwert
    public override int GetHashCode() {
        return HashCode.Combine(Mantissa, Exponent);
    }

    // ============================= Weitere Methoden ===========================================

    public BigNumber Abs() {
        return new BigNumber(Math.Abs(Mantissa), Exponent);
    }

    public bool IsNegative() {
        return Mantissa < 0;
    }

    // Formatieren in lesbare Zahlen
    public string ToDisplayFormat() {
        if(Exponent < 3) return $"{Mantissa * Math.Pow(10, Exponent):0.##}";
        if(Exponent < 6) return $"{Mantissa * Math.Pow(10, Exponent - 3):0.##}K";
        if(Exponent < 9) return $"{Mantissa * Math.Pow(10, Exponent - 6):0.##}M";
        if(Exponent < 12) return $"{Mantissa * Math.Pow(10, Exponent - 9):0.##}B";
        if(Exponent < 15) return $"{Mantissa * Math.Pow(10, Exponent - 12):0.##}T";
        if(Exponent < 18) return $"{Mantissa * Math.Pow(10, Exponent - 15):0.##}Qa";
        if(Exponent < 21) return $"{Mantissa * Math.Pow(10, Exponent - 18):0.##}Qi";
        if(Exponent < 24) return $"{Mantissa * Math.Pow(10, Exponent - 21):0.##}Sx";
        if(Exponent < 27) return $"{Mantissa * Math.Pow(10, Exponent - 24):0.##}Sp";
        if(Exponent < 30) return $"{Mantissa * Math.Pow(10, Exponent - 27):0.##}Oc";
        if(Exponent < 33) return $"{Mantissa * Math.Pow(10, Exponent - 30):0.##}No";
        if(Exponent < 36) return $"{Mantissa * Math.Pow(10, Exponent - 33):0.##}Dc";

        return $"{Mantissa:0.##}e{Exponent}";
    }

    public static BigNumber Parse(string number){
        BigNumber a;
        number = number.ToLower();
        if(number.Contains("e")){
			string[] list = number.Split("e");
			a = new(double.Parse(list[0]), int.Parse(list[1]));
		}else{
			a = new(double.Parse(number));
		}
        return a;
    }
}