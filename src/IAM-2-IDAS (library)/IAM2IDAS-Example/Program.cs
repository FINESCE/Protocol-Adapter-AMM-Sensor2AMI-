using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IAM2IDAS;

    class Program
    {
        static void Main(string[] args)
        {
            //questo indirizzo punta all'istanza dell'idas dedicata a FINESCE (e.idas1.tid@telefonica.com)
            //l'api key è fornita da telefonica
            IDASClient c = new IDASClient("http://130.206.80.47:8002/idas/sml?apikey=69ra4ia6acvgm6f5p7nn19crdi");

            //register meter object first in order to make an entity id meter:0125
            c.registerMeter("9999");
            //register load object in order to make an entity id load:0125
            c.registerLoad("9999");

            //ONLY after registration it is possible to insert observations
            c.insertMeterObservation("9999", (long)1415888468,12.1 ,12.2 ,13.1 ,13.2 ,13.3,13.4);
            c.insertLoadObservation("9999", "2", (long)1415888468, 23.1, 23.2, 23.3, 23.4, 23.5, 23.6, "T1", "IP1", (long)1415888468);
            Console.ReadLine();
            //verifica se è andato a buon fine (Orion di Massimiliano (ENG))
            //POST a http://130.206.82.78:1026/NGSI10/queryContext
            //request body: 
            //{
            //  "entities": [
            //    {
            //      "isPattern": "true",
            //      "id": "meter:0125"
            //     }
            //  ]
            //}

//            POST /NGSI10/queryContext HTTP/1.1
//              Host: 130.206.82.78:1026
//          Content-Type: application/json
//          x-auth-token: Rr1q-xs97xPblm5IBOYgWxclsMJ9zQrQdgX5XoIl-KVMDHYsRlDJ_0LDwOkipII9iCTqhrCahr30Vsn15BYq7w
//          Cache-Control: no-cache

//          { "entities": [ { "isPattern": "true", "id": "meter:0125" } ] } 
        }
    }
