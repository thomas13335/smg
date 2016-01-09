
using System;
using System.IO;
using System.Text;
using SMG.Common;
using SMG.Common.Conditions;
using SMG.Common.Transitions;
using SMG.Common.Types;
using SMG.Common.Effects;


namespace SMG.Compiler
{

public class Parser {
	public const int _EOF = 0;
	public const int _identifier = 1;
	public const int _number = 2;
	public const int maxT = 24;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

	// SMG custom elements
	internal StateMachine SM { get; set; }
	internal ICondition Result { get; set; }



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void SMG() {
		if (la.kind == 3) {
			string name; 
			Get();
			Identifier(out name);
			if(null != SM) { SM.Name = name; } 
			while (StartOf(1)) {
				if (la.kind == 5) {
					Declare();
				} else if (la.kind == 7) {
					Assert();
				} else if (la.kind == 8) {
					Trigger();
				} else {
					Guard();
				}
			}
		} else if (la.kind == 4) {
			Get();
			ICondition c; 
			Condition(out c);
			Result = c; 
		} else SynErr(25);
	}

	void Identifier(out string name) {
		Expect(1);
		name = t.val; 
	}

	void Declare() {
		Expect(5);
		StateType stype; 
		TypeReference(out stype);
		if (la.kind == 1) {
			VariableDefinition(stype);
			while (la.kind == 6) {
				Get();
				VariableDefinition(stype);
			}
		}
	}

	void Assert() {
		ICondition cond; 
		Expect(7);
		Condition(out cond);
		SM.AddAssertion(cond); 
	}

	void Trigger() {
		ICondition cond; string name; Event e; Trigger trigger; EffectList effects;  
		Expect(8);
		Identifier(out name);
		e = SM.AddEvent(name); 
		while (la.kind == 9) {
			Get();
			Condition(out cond);
			trigger = new Trigger(e, cond); 
			if (la.kind == 13 || la.kind == 14) {
				ActionList(out effects);
				trigger.AddEffects(effects); 
			}
			SM.AddTrigger(trigger); 
		}
	}

	void Guard() {
		ICondition cond = null; EffectList effects; string name = null; GuardType gtype = GuardType.Undefined; 
		Expect(10);
		if (la.kind == 1) {
			Identifier(out name);
		}
		Expect(9);
		if (la.kind == 11) {
			Get();
			gtype = GuardType.ENTER; 
		} else if (la.kind == 12) {
			Get();
			gtype = GuardType.LEAVE; 
		} else if (la.kind == 1 || la.kind == 17 || la.kind == 18) {
		} else SynErr(26);
		Condition(out cond);
		Guard guard = SM.AddGuard(cond, gtype, name); 
		if (la.kind == 13 || la.kind == 14) {
			ActionList(out effects);
			guard.AddEffects(effects); 
		}
	}

	void Condition(out ICondition cond) {
		cond = null; ICondition other = null; 
		SimpleCondition(out cond);
		while (la.kind == 15 || la.kind == 16) {
			if (la.kind == 15) {
				Get();
				SimpleCondition(out other);
				cond = cond.ComposeIntersection(other); 
			} else {
				Get();
				SimpleCondition(out other);
				cond = cond.ComposeUnion(other); 
			}
		}
		cond.Freeze(); 
	}

	void TypeReference(out StateType stype) {
		stype = null; string name; 
		Identifier(out name);
		stype = SM.GetStateType(name); 
		if (la.kind == 18) {
			Get();
			if(null != stype) throw new Exception("SMGXXX: type is already defined."); 
			stype = SM.AddSimpleType(name); 
			IdList list; 
			IdentifierList(out list);
			stype.AddStateNames(list); 
			Expect(19);
			stype.Freeze(); 
		}
		if(null == stype) throw new Exception("SMG023: type " + name + " is undefined."); 
	}

	void VariableDefinition(StateType stype) {
		string name; 
		Identifier(out name);
		SM.AddVariable(name, stype); 
	}

	void ActionList(out EffectList actions) {
		actions = new EffectList(); Effect effect; 
		Action(out effect);
		actions.Add(effect); 
		while (la.kind == 13 || la.kind == 14) {
			Action(out effect);
			actions.Add(effect); 
		}
	}

	void Action(out Effect effect) {
		string name; effect = null; 
		if (la.kind == 13) {
			Get();
			Identifier(out name);
			effect = new SendEffect(SM, name); 
		} else if (la.kind == 14) {
			Get();
			Identifier(out name);
			effect = new CallEffect(SM, name); 
		} else SynErr(27);
	}

	void SimpleCondition(out ICondition cond) {
		cond = null; ICondition other = null; 
		if (la.kind == 17) {
			Get();
			SimpleCondition(out other);
			cond = other.Invert(); 
		} else if (la.kind == 1) {
			StateCondition(out cond);
		} else if (la.kind == 18) {
			Get();
			Condition(out cond);
			Expect(19);
		} else SynErr(28);
	}

	void StateCondition(out ICondition cond) {
		string varname; Variable v; StateCondition scond = null; IdList pre, post; 
		cond = null; 
		
		Identifier(out varname);
		v = SM.GetVariable(varname); 
		if (la.kind == 18) {
			Get();
			scond = new StateCondition(v); 
			StateIdentifierList(out pre);
			scond.SetPreStates(pre); 
			if (la.kind == 20) {
				Get();
				StateIdentifierList(out post);
				scond.SetPostStates(post); 
			}
			Expect(19);
		}
		cond = (ICondition)scond ?? new BooleanCondition(v); 
	}

	void StateIdentifierList(out IdList list) {
		list = null; 
		if (la.kind == 1) {
			IdentifierList(out list);
		} else if (la.kind == 21) {
			Get();
			list = new IdList(true); 
		} else if (la.kind == 22) {
			Get();
			list = new IdList("0"); 
		} else if (la.kind == 23) {
			Get();
			list = new IdList("1"); 
		} else SynErr(29);
	}

	void IdentifierList(out IdList list) {
		list = new IdList(); string name; 
		Identifier(out name);
		list.Add(name); 
		while (la.kind == 6) {
			Get();
			Identifier(out name);
			list.Add(name); 
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		SMG();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_T,_x,_T, _T,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "sourcefile({0}, {1}): error: {2}"; // 0=line, 1=column, 2=text
	private StringBuilder sb = new StringBuilder();

	public override string ToString()
	{
		return sb.ToString();
	}

	public Errors()
	{
		errorStream = new StringWriter(sb);
	}

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "identifier expected"; break;
			case 2: s = "number expected"; break;
			case 3: s = "\"smg\" expected"; break;
			case 4: s = "\"eval\" expected"; break;
			case 5: s = "\"declare\" expected"; break;
			case 6: s = "\",\" expected"; break;
			case 7: s = "\"assert\" expected"; break;
			case 8: s = "\"trigger\" expected"; break;
			case 9: s = "\"when\" expected"; break;
			case 10: s = "\"guard\" expected"; break;
			case 11: s = "\"enter\" expected"; break;
			case 12: s = "\"leave\" expected"; break;
			case 13: s = "\"send\" expected"; break;
			case 14: s = "\"call\" expected"; break;
			case 15: s = "\"and\" expected"; break;
			case 16: s = "\"or\" expected"; break;
			case 17: s = "\"not\" expected"; break;
			case 18: s = "\"(\" expected"; break;
			case 19: s = "\")\" expected"; break;
			case 20: s = "\"=>\" expected"; break;
			case 21: s = "\"*\" expected"; break;
			case 22: s = "\"0\" expected"; break;
			case 23: s = "\"1\" expected"; break;
			case 24: s = "??? expected"; break;
			case 25: s = "invalid SMG"; break;
			case 26: s = "invalid Guard"; break;
			case 27: s = "invalid Action"; break;
			case 28: s = "invalid SimpleCondition"; break;
			case 29: s = "invalid StateIdentifierList"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}

} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}

}