using System;
using System.Xml.Linq;

internal interface IESaveAndLoad
{
    Type GetClassName();
    XElement GetXElement();
    void RecoverFromXElement(XElement xElement);
}