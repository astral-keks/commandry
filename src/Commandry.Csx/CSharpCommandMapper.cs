using AutoMapper;

namespace Commandry
{
    internal static class CSharpCommandMapper
    {
        private static readonly Mapper _mapper = new Mapper(new MapperConfiguration(cfg => { }));

        public static Command MapParameters(dynamic parameters, Command command)
        {
            return _mapper.Map(parameters, command);
        }
    }
}
