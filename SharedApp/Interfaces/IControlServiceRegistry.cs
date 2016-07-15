namespace SharedApp.Interfaces
{
  public interface IControlServiceRegistry
  {
    void RegisterService<T, U>(U implementation)
      where T : class
      where U : class, T;
    bool HasService<T>() where T : class;
    T GetService<T>() where T : class;
  }
}
