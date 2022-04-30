using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IStringLoadable{

    public string Store();
    public IStringLoadable Restore();

}

