using System;
using System.Collections.Generic;
using System.Text;

namespace CachingDemo
{
    public interface IResource <T>
    {
        T GetResource();
    }
}
