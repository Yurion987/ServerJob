using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;

namespace ServerJob
{
    class Databazka
    {
        OracleConnection conection;

        public Databazka()
        {
            string conStr = "Data Source=(DESCRIPTION =(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST = obelix.fri.uniza.sk)(PORT = 1521)))"
                + "(CONNECT_DATA =(SERVICE_NAME = orcl2.fri.uniza.sk)));User ID=uhrin9;Password=dodo2331996;";
            this.conection = new OracleConnection(conStr);
            try
            {
                this.conection = new OracleConnection(conStr);
                this.conection.Open();
            }
            catch (Exception e)
            {
            }
        }
        public void InsertData(List<Zaznam> data)
        {
            OracleCommand orclCom = new OracleCommand();
            orclCom.Connection = conection;
            OracleDataReader orclReader = null;
            foreach (var zaznam in data)
            {
                if (kontrolaOriginalityMena(zaznam.Meno))
                {
                    orclCom.CommandText = "insert into pouzivatel (meno,heslo) values('" + zaznam.Meno + "',' ')";
                    orclCom.ExecuteNonQuery();
                }
                orclCom.CommandText = "select id_pouzivatela from pouzivatel where meno = '" + zaznam.Meno + "'";
                orclReader = orclCom.ExecuteReader();
                if (orclReader.Read())
                {
                    int IDPouzivatela = Int32.Parse(orclReader["id_pouzivatela"].ToString());
                    if (IDPouzivatela != -1 && kontrolaOriginalityZaznamu(zaznam.Cas, zaznam.Datum, IDPouzivatela))
                    {
                        orclCom.CommandText = "select typ " +
                                 "from ZAZNAM zm join pouzivatel p on(p.ID_pouzivatela = zm.pouzivatel) " +
                                 "where to_char(TO_DATE(sysdate),'dd.mm.yyyy') = to_char(zm.cas, 'dd.mm.yyyy') " +
                                 "and p.meno = '" + zaznam.Meno + "' " +
                                 "order by cas desc " +
                                 "fetch first 1 row only ";
                        orclReader = orclCom.ExecuteReader();
                        if (orclReader.Read())
                        {
                            var typPrichodu = orclReader["TYP"].ToString().Replace(" ", "");
                            if (typPrichodu.Equals("prichod"))
                            {
                                zaznam.Typ = "odchod";
                            }
                            else
                            {
                                zaznam.Typ = "prichod";
                            }
                        }
                        else
                        {
                            zaznam.Typ = "prichod";
                        }
                        orclCom.CommandText = "insert into zaznam (pouzivatel,cas,typ) " +
                            "values (" + IDPouzivatela + ",TO_DATE('" + zaznam.Datum + " " + zaznam.Cas + "','dd.mm.yyyy hh24:mi'),'" + zaznam.Typ + "')";
                        orclCom.ExecuteNonQuery();

                    }
                    else
                    {
                        orclReader.Close();
                       // break;
                    }
                }
                orclReader.Close();
            }
        }
        public void odpoj()
        {
            conection.Clone();
        }
        private bool kontrolaOriginalityMena(string data)
        {

            string comand = "select * from pouzivatel where meno = '" + data + "'";
            OracleCommand orclCom = new OracleCommand(comand, conection);

            OracleDataReader orclReader = orclCom.ExecuteReader();
            if (orclReader.HasRows)
            {
                orclReader.Close();
                return false;
            }
            orclReader.Close();
            return true;
        }
        private bool kontrolaOriginalityZaznamu(string cas, string datum, int ID)
        {
            string comand = "select * from zaznam where cas = TO_DATE('" + datum + " " + cas + "','dd.mm.yyyy hh24:mi') and pouzivatel = " + ID + "";
            OracleCommand orclCom = new OracleCommand(comand, conection);

            OracleDataReader orclReader = orclCom.ExecuteReader();
            if (orclReader.HasRows)
            {
                orclReader.Close();
                return false;
            }
            orclReader.Close();
            return true;
        }
    }
}
