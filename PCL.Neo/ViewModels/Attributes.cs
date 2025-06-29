using System;

namespace PCL.Neo.ViewModels;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class SubViewModelAttribute(Type mainViewModelType) : Attribute
{
    public Type MainViewModelType { get; } = mainViewModelType;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class MainViewModelAttribute(Type defaultSubViewModelType) : Attribute
{
    public Type DefaultSubViewModelType { get; } = defaultSubViewModelType;
}