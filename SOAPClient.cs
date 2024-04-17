// Hemoeco Renta 
// ejemplo de llamada a 'ConsultaCFDIService' del SAT
//////////////////////////////////////////////////////
///
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

// Referencias utilizadas para completar este proyecto:
//
// http://omawww.sat.gob.mx/tramitesyservicios/Paginas/anexo_20.htm
// https://www.sat.gob.mx/consultas/20585/conoce-los-servicios-especializados-de-validacion
// http://omawww.sat.gob.mx/factura/Paginas/documentos/cancelacion/consulta_cfdi.pdf
//
// https://developers.sw.com.mx/knowledge-base/servicio-publico-de-consulta-estatus-cfdi-sat/#
//     https://www.postman.com/red-shadow-569412/workspace/sw-api/request/15933150-5d6d2fac-6f9d-4b07-b28c-2492fcbc3305?ctx=code&tab=headers
//
// https://www.linkedin.com/pulse/como-validar-facturas-en-el-sat-con-web-services-espino-rivera/
//
// https://cfdiutils.readthedocs.io/es/latest/componentes/estado-sat.html
//
// https://github.com/sf-solutionfactory/PortalProvGONHER/blob/master
//     https://consultaqr.facturaelectronica.sat.gob.mx/consultacfdiservice.svc?wsdl
//     https://consultaqr.facturaelectronica.sat.gob.mx/ConsultaCFDIService.svc?xsd=xsd0
//
// https://stackoverflow.com/questions/9343594/how-to-call-asynchronous-method-from-synchronous-method-in-c
//    https://blog.stephencleary.com/2012/02/async-and-await.html
//
// https://cfdiutils.readthedocs.io/es/latest/
//
// https://github.com/egalink/ConsultaCFDIService


public class SoapClient
{
    public class ParametroLlamada
    {
        public ParametroLlamada(string nombre, string descripcion)
        {
            Nombre = nombre;
            Descripcion = descripcion;
        }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string Valor { get; set; }


        public string QueryParam => $"{Nombre}={Valor}";
    }

    public static void Main()
    {
        
        try
        {
            List<ParametroLlamada> entradas = new List<ParametroLlamada>()
            {
                { new ParametroLlamada("re", "RFC de emisor") },
                { new ParametroLlamada("rr", "RFC de receptor") },
                { new ParametroLlamada("tt", "total de factura") }, // ej: 27240.00
                { new ParametroLlamada("id", "UUID") }
            };

            // get required values
            foreach(ParametroLlamada pl in entradas)
            {
                Console.Write($"Capture por favor el {pl.Descripcion}: ");
                pl.Valor = Console.ReadLine();
            }

            // Thanks to Lee Smith answer in https://stackoverflow.com/questions/9343594/how-to-call-asynchronous-method-from-synchronous-method-in-c
            MainAsync(entradas).GetAwaiter().GetResult();
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }

    public static async Task MainAsync(List<ParametroLlamada> entradas)
    {
        var url = "https://consultaqr.facturaelectronica.sat.gob.mx/ConsultaCFDIService.svc";
        var action = "http://tempuri.org/IConsultaCFDIService/Consulta";

        var soapEnvelopeXml = CreateSoapEnvelope(entradas);
        var request = CreateWebRequest(url, action, entradas);

        var client = new HttpClient();

        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        Console.WriteLine(response.StatusCode);

        Console.WriteLine(response.Content.ReadAsStringAsync().Result);
    }

    private static HttpRequestMessage CreateWebRequest(string url, string action, List<ParametroLlamada> entradas)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Add("Accept", "text/xml");
        request.Headers.Add("SOAPAction", action);

        request.Content = new StringContent(CreateSoapEnvelope(entradas), null, "text/xml");

        return request;
    }

    private static string CreateSoapEnvelope(List<ParametroLlamada> entradas)
    {
        return $@"
            <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
            <soapenv:Header/>
            <soapenv:Body>
                <tem:Consulta>
                    <!--Optional:-->
                    <tem:expresionImpresa>
                        <![CDATA[{ToQueryString(entradas)}]]>
                    </tem:expresionImpresa>
                </tem:Consulta>
            </soapenv:Body>
        </soapenv:Envelope>";
    }

    private static string ToQueryString(List<ParametroLlamada> entradas)
    {
        // modified from annaka answer in https://stackoverflow.com/questions/829080/how-to-build-a-query-string-for-a-url-in-c
        var array = entradas.Select(pl => pl.QueryParam).ToArray();
        return "?" + string.Join("&", array);
    }
}