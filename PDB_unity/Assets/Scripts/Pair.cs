using System;

public class Pair<T1, T2> {
	private T1 first;
	private T2 second;

	public T1 First { 
		get { return first; }
		set { first = value; }
	}

	public T2 Second { 
		get { return second; }
		set { second = value; }
	}

	public Pair(T1 first, T2 second) {
		this.first = first;
		this.second = second;
	}

	public Pair() {
	}

	public static bool operator==(Pair<T1, T2> a, Pair<T1, T2> b) {
		return (a.first.Equals(b.first) && a.second.Equals(b.second));            
	}
	
	public static bool operator!=(Pair<T1, T2> a, Pair<T1, T2> b) {
		return !(a == b);
	}

	public override bool Equals(object o) {
		if (o.GetType() != typeof(Pair<T1, T2>)) {
			return false;
		}
		Pair<T1, T2> other = (Pair<T1, T2>) o;
		return this == other;
	}

	// From Josh Bloch in Effective Java
	public override int GetHashCode() {
		int hash = 17;
		hash = 31 * hash + first.GetHashCode();
		hash = 31 * hash + second.GetHashCode();
		return hash;
	}

	public override string ToString() {
		return string.Format("Tuple({0}, {1})", first, second);
	}
}
