using Parkking.Models.Cobro;
using Parkking.Models.Enums;

namespace Parkking.Models.Tests;

public class AgrupacionMensualStrategyTests
{
    private static readonly DateOnly AnclaAgo = new(2026, 8, 1);

    [Fact]
    public void Mensual_CicloQueContiene_EsElMes()
    {
        var s = new AgrupacionMensualStrategy(1);
        var p = s.CicloQueContiene(new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 1));
        Assert.Equal(new DateOnly(2026, 9, 1), p.Inicio);
        Assert.Equal(new DateOnly(2026, 9, 30), p.Fin);
    }

    [Fact]
    public void Trimestral_AncladoAgosto_NoEsCalendarioAnual()
    {
        var s = new AgrupacionMensualStrategy(3);
        var p = s.CicloQueContiene(new DateOnly(2026, 8, 20), AnclaAgo);
        Assert.Equal(new DateOnly(2026, 8, 1), p.Inicio);
        Assert.Equal(new DateOnly(2026, 10, 31), p.Fin);
    }

    [Fact]
    public void Trimestral_Prorratear_20Ago()
    {
        var s = new AgrupacionMensualStrategy(3);
        var r = s.ResolverPrimerPeriodo(
            new DateOnly(2026, 8, 20),
            PoliticaPrimerPeriodo.Prorratear,
            300m,
            AnclaAgo);

        Assert.Equal(new DateOnly(2026, 8, 20), r.Periodo.Inicio);
        Assert.Equal(new DateOnly(2026, 10, 31), r.Periodo.Fin);
        Assert.True(r.FueProrrateado);
        // (12/31)*100 + 100 + 100 ≈ 38.7 + 200 → 239
        Assert.Equal(239m, r.Monto);
    }

    [Fact]
    public void Trimestral_OmitirMesEntrante_20Ago_CobraSepOct()
    {
        var s = new AgrupacionMensualStrategy(3);
        var r = s.ResolverPrimerPeriodo(
            new DateOnly(2026, 8, 20),
            PoliticaPrimerPeriodo.OmitirMesEntrante,
            300m,
            AnclaAgo);

        Assert.Equal(new DateOnly(2026, 9, 1), r.Periodo.Inicio);
        Assert.Equal(new DateOnly(2026, 10, 31), r.Periodo.Fin);
        Assert.Equal(200m, r.Monto);
        Assert.False(r.FueProrrateado);
    }

    [Fact]
    public void Trimestral_Completo_20Ago()
    {
        var s = new AgrupacionMensualStrategy(3);
        var r = s.ResolverPrimerPeriodo(
            new DateOnly(2026, 8, 20),
            PoliticaPrimerPeriodo.PeriodoCompleto,
            300m,
            AnclaAgo);

        Assert.Equal(new DateOnly(2026, 8, 20), r.Periodo.Inicio);
        Assert.Equal(new DateOnly(2026, 10, 31), r.Periodo.Fin);
        Assert.Equal(300m, r.Monto);
    }

    [Fact]
    public void Mensual_OmitirMesEntrante_SaltaAlMesSiguiente()
    {
        var s = new AgrupacionMensualStrategy(1);
        var ancla = new DateOnly(2026, 9, 1);
        var r = s.ResolverPrimerPeriodo(
            new DateOnly(2026, 9, 10),
            PoliticaPrimerPeriodo.OmitirMesEntrante,
            100m,
            ancla);

        Assert.Equal(new DateOnly(2026, 10, 1), r.Periodo.Inicio);
        Assert.Equal(new DateOnly(2026, 10, 31), r.Periodo.Fin);
        Assert.Equal(100m, r.Monto);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Dia1o2_Prorratear_SeTrataComoCompleto(int dia)
    {
        var s = new AgrupacionMensualStrategy(1);
        var fecha = new DateOnly(2026, 9, dia);
        var r = s.ResolverPrimerPeriodo(fecha, PoliticaPrimerPeriodo.Prorratear, 100m, fecha);

        Assert.False(r.FueProrrateado);
        Assert.Equal(100m, r.Monto);
        Assert.Equal(fecha, r.Periodo.Inicio);
        Assert.Equal(new DateOnly(2026, 9, 30), r.Periodo.Fin);
    }

    [Fact]
    public void Factory_Semestral_UsaN6()
    {
        var s = PeriodicidadStrategyFactory.For(PeriodicidadCobro.Semestral);
        Assert.Equal(6, s.MesesPorCiclo);
        var ancla = new DateOnly(2026, 8, 1);
        var p = s.CicloQueContiene(new DateOnly(2026, 8, 20), ancla);
        Assert.Equal(new DateOnly(2026, 8, 1), p.Inicio);
        Assert.Equal(new DateOnly(2027, 1, 31), p.Fin);
    }
}

public class QuincenalStrategyTests
{
    [Fact]
    public void Slot_PrimeraQuincena()
    {
        var s = new QuincenalStrategy();
        var p = s.CicloQueContiene(new DateOnly(2026, 9, 10), default);
        Assert.Equal(new DateOnly(2026, 9, 1), p.Inicio);
        Assert.Equal(new DateOnly(2026, 9, 15), p.Fin);
    }

    [Fact]
    public void OmitirMesEntrante_SaltaASiguienteQuincena()
    {
        var s = new QuincenalStrategy();
        var r = s.ResolverPrimerPeriodo(
            new DateOnly(2026, 9, 10),
            PoliticaPrimerPeriodo.OmitirMesEntrante,
            50m,
            default);

        Assert.Equal(new DateOnly(2026, 9, 16), r.Periodo.Inicio);
        Assert.Equal(new DateOnly(2026, 9, 30), r.Periodo.Fin);
        Assert.Equal(50m, r.Monto);
    }
}

public class AnualStrategyTests
{
    [Fact]
    public void Ciclo_AncladoAbril_NoEsEneDic()
    {
        var s = new AnualStrategy();
        var ancla = new DateOnly(2026, 4, 1);
        var p = s.CicloQueContiene(new DateOnly(2026, 4, 16), ancla);
        Assert.Equal(new DateOnly(2026, 4, 1), p.Inicio);
        Assert.Equal(new DateOnly(2027, 3, 31), p.Fin);
    }

    [Fact]
    public void OmitirMesEntrante_SaltaAlSiguienteAnioCiclo()
    {
        var s = new AnualStrategy();
        var ancla = new DateOnly(2026, 4, 1);
        var r = s.ResolverPrimerPeriodo(
            new DateOnly(2026, 4, 16),
            PoliticaPrimerPeriodo.OmitirMesEntrante,
            1200m,
            ancla);

        Assert.Equal(new DateOnly(2027, 4, 1), r.Periodo.Inicio);
        Assert.Equal(new DateOnly(2028, 3, 31), r.Periodo.Fin);
        Assert.Equal(1200m, r.Monto);
    }
}
