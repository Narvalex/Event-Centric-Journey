using System;
using Xunit;

namespace Journey.Tests.EventSourcing.DaoFixture
{
    public class DADO_un_func
    {
        [Fact]
        public void CUANDO_ejecuto_un_func_con_un_solo_parametro_ENTONCES_me_devuelve_el_objecto_definido()
        {
            Func<string> func = new Func<string>(() => { return "hola"; });
            Assert.Equal("hola", func());
        }

        [Fact]
        public void CUANDO_ejecuto_func_con_dos_parametros_ENTONCES_aplica_el_resultado_al_segundo_parametro()
        {
            Func<string, int> func = new Func<string, int>((texto) => { return texto.Length; });
            var resultado = func("hola");
            Assert.Equal(4, resultado);
        }
    }
}
