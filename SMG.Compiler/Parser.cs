
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using SMG.Common;
using SMG.Common.Conditions;
using SMG.Common.Transitions;
using SMG.Common.Types;
using SMG.Common.Effects;
using SMG.Common.Code;
using SMG.Common.Exceptions;


namespace SMG.Compiler
{

public class Parser {
	public const int _EOF = 0;
	public const int _identifier = 1;
	public const int _number = 2;
	public const int _string = 3;
	public const int maxT = 32;

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
	internal CodeLocation Location { get { return new CodeLocation(scanner.Line, scanner.Column); } }
	internal CodeParameters Parameters { get; set; }

	public event SyntaxErrorHandler OnSyntaxError;

	internal void TriggerError(Exception ex)
	{
		if(null != OnSyntaxError) 
		{
			OnSyntaxError(this, new SyntaxErrorEventArgs(ex));
		}
	}

	private void UpdateLocation()
	{
		if(null != SM) 
		{
			SM.SetLocation(Location);
		}
	}



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors(this);
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
		if (la.kind == 4) {
			string name; 
			Get();
			Identifier(out name);
			if(null != SM) { SM.Name = name; } 
			
			UpdateLocation();
			
			Options();
			while (StartOf(1)) {
				if (la.kind == 12) {
					UpdateLocation(); 
					Declare();
				} else if (la.kind == 14) {
					Assert();
				} else if (la.kind == 15) {
					Trigger();
				} else {
					Guard();
				}
			}
		} else if (la.kind == 5) {
			Get();
			ICondition c; 
			Condition(out c);
			Result = c; 
		} else SynErr(33);
	}

	void Identifier(out string name) {
		Expect(1);
		name = t.val; 
	}

	void Options() {
		if (la.kind == 6) {
			Get();
			IdList idlist; 
			if (la.kind == 1) {
				DottedList(out idlist);
				Parameters.Namespace = idlist.ToNamespace(); 
			} else if (la.kind == 3) {
				Get();
				Parameters.Namespace = t.val.Trim(new char[] { '\"' }); 
			} else SynErr(34);
		}
		if (la.kind == 7) {
			Get();
			IdList idlist; 
			DottedList(out idlist);
			Parameters.BaseClassName = idlist.ToNamespace(); 
		}
		if (la.kind == 8) {
			Get();
			if (la.kind == 9) {
				Get();
				Parameters.Language = "C#"; 
			} else if (la.kind == 10) {
				Get();
				Parameters.Language = "jscript"; 
			} else SynErr(35);
		}
		if (la.kind == 11) {
			Get();
			Parameters.IsPartial = true; 
		}
	}

	void Declare() {
		Expect(12);
		StateType stype; 
		TypeReference(out stype);
		if (la.kind == 1) {
			VariableDefinition(stype);
			while (la.kind == 13) {
				Get();
				VariableDefinition(stype);
			}
		}
	}

	void Assert() {
		ICondition cond; 
		Expect(14);
		Condition(out cond);
		SM.AddAssertion(cond); 
	}

	void Trigger() {
		ICondition cond; 
		string name; 
		Event e; 
		Trigger trigger; 
		EffectList effects; 
		
		UpdateLocation();
		
		Expect(15);
		Identifier(out name);
		e = SM.AddEvent(name); 
		while (la.kind == 16) {
			Get();
			Condition(out cond);
			trigger = new Trigger(e, cond); 
			if (la.kind == 20 || la.kind == 21) {
				ActionList(out effects);
				trigger.AddEffects(effects); 
			}
			UpdateLocation();
			SM.AddTrigger(trigger); 
			
		}
	}

	void Guard() {
		ICondition cond = null; EffectList effects; string name = null; GuardType gtype = GuardType.Undefined; 
		Expect(17);
		if (la.kind == 1) {
			Identifier(out name);
		}
		Expect(16);
		if (la.kind == 18) {
			Get();
			gtype = GuardType.ENTER; 
		} else if (la.kind == 19) {
			Get();
			gtype = GuardType.LEAVE; 
		} else if (la.kind == 1 || la.kind == 24 || la.kind == 25) {
		} else SynErr(36);
		Condition(out cond);
		Guard guard = SM.AddGuard(cond, gtype, name); 
		if (la.kind == 20 || la.kind == 21) {
			ActionList(out effects);
			guard.AddEffects(effects); 
		}
	}

	void Condition(out ICondition cond) {
		cond = null; ICondition other = null; 
		SimpleCondition(out cond);
		while (la.kind == 22 || la.kind == 23) {
			if (la.kind == 22) {
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

	void DottedList(out IdList list) {
		list = new IdList(); string name; 
		Identifier(out name);
		list.Add(name); 
		while (la.kind == 31) {
			Get();
			Identifier(out name);
			list.Add(name); 
		}
	}

	void TypeReference(out StateType stype) {
		stype = null; string name; 
		Identifier(out name);
		stype = SM.GetStateType(name); 
		if (la.kind == 25) {
			Get();
			if(null != stype) 
			throw new CompilerException(ErrorCode.TypeRedefinition, 
			"type '" + name + "' is already defined."); 
			
			stype = SM.AddSimpleType(name); 
			IdList list; 
			IdentifierList(out list);
			stype.AddStateNames(list); 
			Expect(26);
			stype.Freeze(); 
		}
		if(null == stype)
		throw new CompilerException(ErrorCode.UndefinedType, 
		"type '" + name + "' is undefined."); 
		
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
		while (la.kind == 20 || la.kind == 21) {
			Action(out effect);
			actions.Add(effect); 
		}
	}

	void Action(out Effect effect) {
		string name; effect = null; 
		if (la.kind == 20) {
			Get();
			Identifier(out name);
			effect = new SendEffect(SM, name); 
		} else if (la.kind == 21) {
			Get();
			Identifier(out name);
			effect = new CallEffect(SM, name); 
		} else SynErr(37);
	}

	void SimpleCondition(out ICondition cond) {
		cond = null; ICondition other = null; 
		if (la.kind == 24) {
			Get();
			SimpleCondition(out other);
			cond = other.Invert(); 
		} else if (la.kind == 1) {
			StateCondition(out cond);
		} else if (la.kind == 25) {
			Get();
			Condition(out cond);
			Expect(26);
		} else SynErr(38);
	}

	void StateCondition(out ICondition cond) {
		string varname; Variable v; StateCondition scond = null; IdList pre, post; 
		cond = null; 
		
		Identifier(out varname);
		v = SM.GetVariable(varname); 
		if (la.kind == 25) {
			Get();
			scond = new StateCondition(v); 
			StateIdentifierList(out pre);
			scond.SetPreStates(pre); 
			if (la.kind == 27) {
				Get();
				StateIdentifierList(out post);
				scond.SetPostStates(post); 
			}
			Expect(26);
		}
		cond = (ICondition)scond ?? new BooleanCondition(v); 
	}

	void StateIdentifierList(out IdList list) {
		list = null; 
		if (la.kind == 1) {
			IdentifierList(out list);
		} else if (la.kind == 28) {
			Get();
			list = new IdList(true); 
		} else if (la.kind == 29) {
			Get();
			list = new IdList("0"); 
		} else if (la.kind == 30) {
			Get();
			list = new IdList("1"); 
		} else SynErr(39);
	}

	void IdentifierList(out IdList list) {
		list = new IdList(); string name; 
		Identifier(out name);
		list.Add(name); 
		while (la.kind == 13) {
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
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_T,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x}

	};
} // end Parser



public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "sourcefile({0}, {1}): error: {2}"; // 0=line, 1=column, 2=text
	private StringBuilder sb = new StringBuilder();
	private Parser parser;

	public override string ToString()
	{
		return sb.ToString();
	}

	public Errors(Parser parser)
	{
		this.parser = parser;
		errorStream = new StringWriter(sb);
	}

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "identifier expected"; break;
			case 2: s = "number expected"; break;
			case 3: s = "string expected"; break;
			case 4: s = "\"smg\" expected"; break;
			case 5: s = "\"eval\" expected"; break;
			case 6: s = "\"namespace\" expected"; break;
			case 7: s = "\"base\" expected"; break;
			case 8: s = "\"language\" expected"; break;
			case 9: s = "\"c#\" expected"; break;
			case 10: s = "\"jscript\" expected"; break;
			case 11: s = "\"partial\" expected"; break;
			case 12: s = "\"declare\" expected"; break;
			case 13: s = "\",\" expected"; break;
			case 14: s = "\"assert\" expected"; break;
			case 15: s = "\"trigger\" expected"; break;
			case 16: s = "\"when\" expected"; break;
			case 17: s = "\"guard\" expected"; break;
			case 18: s = "\"enter\" expected"; break;
			case 19: s = "\"leave\" expected"; break;
			case 20: s = "\"send\" expected"; break;
			case 21: s = "\"call\" expected"; break;
			case 22: s = "\"and\" expected"; break;
			case 23: s = "\"or\" expected"; break;
			case 24: s = "\"not\" expected"; break;
			case 25: s = "\"(\" expected"; break;
			case 26: s = "\")\" expected"; break;
			case 27: s = "\"=>\" expected"; break;
			case 28: s = "\"*\" expected"; break;
			case 29: s = "\"0\" expected"; break;
			case 30: s = "\"1\" expected"; break;
			case 31: s = "\".\" expected"; break;
			case 32: s = "??? expected"; break;
			case 33: s = "invalid SMG"; break;
			case 34: s = "invalid Options"; break;
			case 35: s = "invalid Options"; break;
			case 36: s = "invalid Guard"; break;
			case 37: s = "invalid Action"; break;
			case 38: s = "invalid SimpleCondition"; break;
			case 39: s = "invalid StateIdentifierList"; break;

			default: s = "error " + n; break;
		}
		//errorStream.WriteLine(errMsgFormat, line, col, s);
		//_errors.Add();
		var ex = new SyntaxErrorException(new CodeLocation(line, col), s);
		parser.TriggerError(ex);
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