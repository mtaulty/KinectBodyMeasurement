namespace Measurements
{
  using System;
  using System.Collections.Generic;
  using System.Text;

  public class NamedValue  
  {
    private NamedValue ()
	  {
	  }
    public NamedValue(string name)
    {
      this.Name = name;
    }
    public NamedValue(string name, double value) : this(name)
    {
      this.Value = value;
    }
    public string Name { get; internal set; }

    public virtual double Value
    {
      get
      {
        return (this.value);
      }
      protected set
      {
        this.value = value;
      }
    }
    double value;

    public string DisplayString
    {
      get
      {
        return (this.Value.ToString("G5"));
      }
    }
  }
}
