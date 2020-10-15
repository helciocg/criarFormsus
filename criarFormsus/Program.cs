using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace criarFormsus
{
    class Program
    {
        static void Main(string[] args)
        {

            // Create new stopwatch
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            // Begin timing
            stopwatch.Start();

            var bookList = new List<ObjCP>();
            string line;

            using (StreamReader reader = new StreamReader(@"c:\si\listaCPs.txt"))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    var listaCP = line.Split('|');

                    if (line != string.Empty)
                    {
                        bookList.Add(new ObjCP{
                            strNuCP = listaCP[0],
                            strNomeTecnologia = listaCP[1],
                            strNomeTecnologiaCurta = listaCP[2],
                            dtInicio = listaCP[3],
                            dtFim = listaCP[4],
                            strFavNaoFav = listaCP[5]
                        });

                    }

                }
                
            }

            //abrir o firefox
            IWebDriver driver = new FirefoxDriver();

            //Fazer o login
            logar(driver);

            int count = 0;
            foreach (var item in bookList)
            {
                count++;
                FormulariosCP formulariosCP = CriarFormularios(driver);

                Console.WriteLine("Nu Formulário Técnico: {0}", formulariosCP.StrNuTecnico);
                Console.WriteLine("Nu Formulário Opinião: {0}", formulariosCP.StrNuOpiniao);
                item.iDNuTecnico = formulariosCP.StrNuTecnico;
                item.iDNuOpiniao = formulariosCP.StrNuOpiniao;

                if (count % 2 == 0)
                {
                    Console.WriteLine("Relogando: {0}", count.ToString());
                    logar(driver);
                }

            }

            foreach (var item in bookList)
            {

                Console.WriteLine("New Item---");
                Console.WriteLine("strNuCP = {0}", item.strNuCP);
                Console.WriteLine("strNomeTecnologia = {0}", item.strNomeTecnologia);
                Console.WriteLine("strNomeTecnologiaCurta = {0}", item.strNomeTecnologiaCurta);
                Console.WriteLine("dtInicio = {0}", item.dtInicio);
                Console.WriteLine("dtFim = {0}", item.dtFim);
                Console.WriteLine("strFavNaoFav = {0}", item.strFavNaoFav);
                Console.WriteLine("iDNuTecnico = {0}", item.iDNuTecnico);
                Console.WriteLine("iDNuOpiniao = {0}", item.iDNuOpiniao);
                Console.WriteLine("End Item---");
                Console.WriteLine("");

            }
            //Console.Read();

            #region debug
            /*
            ObjCP nova = new ObjCP
            {
                strNuCP = "74/2018",
                strNomeTecnologia = "do alfaeftrenonacogue no tratamento da hemofilia B",
                strNomeTecnologiaCurta = "Alfaeftrenonacogue para hemofilia B",
                dtInicio = "29/11/2018",
                dtFim = "18/12/2018",
                strFavNaoFav = "Não favorável",
                iDNuTecnico = formulariosCP.StrNuTecnico,
                iDNuOpiniao = formulariosCP.StrNuOpiniao
            };
            */
            #endregion

            foreach (var item in bookList)
            {
                CriarCP(item, driver, "Tecnico");
                CriarCP(item, driver, "Opiniao");
            }

            //Fechar o firefox
            driver.Quit();

            // Stop timing
            stopwatch.Stop();

            //Console.WriteLine("Time taken : {0}", stopwatch.Elapsed);

            Console.WriteLine("Programa finalizdo com sucesso!!!! Tempo: {0}", stopwatch.Elapsed);
            Console.Read();
        }

        private static void logar(IWebDriver driver)
        {
            #region Logar

            driver.Navigate().GoToUrl("http://formsus.datasus.gov.br/site/default.php?login_usuario=LOGOFF");

            IWebElement userElement = driver.FindElement(By.Name("login_usuario"));
            userElement.SendKeys("conitec");

            IWebElement passElement = driver.FindElement(By.Name("senha_usuario"));
            passElement.SendKeys("conitec7646");

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("document.form.submit()");
            //Thread.Sleep(2000);
            #endregion
        }

        private static FormulariosCP CriarFormularios(IWebDriver driver)
        {
            Console.WriteLine("Copiando formulário Técnico");
            driver.Navigate().GoToUrl("http://formsus.datasus.gov.br/admin/aplicacao.php?acao=copiar&id_aplicacao=50436");
            Thread.Sleep(1000);

            Console.WriteLine("Copiando formulário Opinião");
            driver.Navigate().GoToUrl("http://formsus.datasus.gov.br/admin/aplicacao.php?acao=copiar&id_aplicacao=50437");
            Thread.Sleep(1000);

            Console.WriteLine("Indo para página principal");
            driver.Navigate().GoToUrl("http://formsus.datasus.gov.br/admin/aplicacao.php");

            IWebElement pesquisaElement;
            while (true)
            {
                try
                {
                    pesquisaElement = driver.FindElement(By.Name("filtro_nome_aplicacao"));
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(5000);
                }
            }

            pesquisaElement.Clear();
            pesquisaElement.SendKeys("Técnico - Base - NÃO apagar (Cópia");

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("enviar_filtro(); void(null);");
            Thread.Sleep(2000);

            string idElementTecnico = driver.FindElement(By.ClassName("conteudo_peq")).Text;
            idElementTecnico = idElementTecnico.Substring(idElementTecnico.Length - 5);
            //Console.WriteLine(idElementTecnico);

            pesquisaElement = driver.FindElement(By.Name("filtro_nome_aplicacao"));
            pesquisaElement.Clear();
            pesquisaElement.SendKeys("Opinião - Base - NÃO apagar (Cópia");
            js.ExecuteScript("enviar_filtro(); void(null);");
            Thread.Sleep(2000);

            string idElementOpiniao = driver.FindElement(By.ClassName("conteudo_peq")).Text;
            idElementOpiniao = idElementOpiniao.Substring(idElementOpiniao.Length - 5);
            //Console.WriteLine(idElementOpiniao);

            FormulariosCP formulariosCP = new FormulariosCP
            {
                StrNuTecnico = idElementTecnico,
                StrNuOpiniao = idElementOpiniao
            };

            return formulariosCP;
        }

        private static void CriarCP(ObjCP novaCP, IWebDriver driver,string strTipo)
        {
            string iDNu;
            string strNomeTipo;
            string strFavNaoFavNovo;

            if (strTipo == "Tecnico") {
                iDNu = novaCP.iDNuTecnico;
                strNomeTipo = "Técnico";
            }
            else {
                iDNu = novaCP.iDNuOpiniao;
                strNomeTipo = "Opinião";
            }
          
            driver.Navigate().GoToUrl("http://formsus.datasus.gov.br/admin/aplicacao.php?acao=alterar&id_aplicacao=" + iDNu);
            //Thread.Sleep(2000);

            string strTemp;
            strTemp = driver.FindElement(By.Name("nome_aplicacao")).GetAttribute("value");
            strTemp = "Consulta Pública Conitec/ SCTIE Nº " + novaCP.strNuCP + " - "+ strNomeTipo + " - " + novaCP.strNomeTecnologiaCurta;

            IWebElement pesquisaElement = driver.FindElement(By.Name("nome_aplicacao"));
            pesquisaElement.Clear();
            pesquisaElement.SendKeys(strTemp);

            pesquisaElement = driver.FindElement(By.Name("data_inicio_aplicacao"));
            pesquisaElement.Clear();
            pesquisaElement.SendKeys(novaCP.dtInicio);

            pesquisaElement = driver.FindElement(By.Name("data_fim_aplicacao"));
            pesquisaElement.Clear();
            pesquisaElement.SendKeys(novaCP.dtFim);
            
            //---------------------------------------------------------------------------------------------------------------
            IWebElement iframeMsg = driver.FindElement(By.Id("mce_editor_2"));
            driver.SwitchTo().Frame(iframeMsg);

            if (novaCP.strNomeTecnologia.IndexOf("ampliação de uso") > 0)
            {
                strFavNaoFavNovo = novaCP.strFavNaoFav + " à ampliação de uso";
            }
           
            else if(novaCP.strNomeTecnologia.IndexOf("exclusão") > 0)
            {
                strFavNaoFavNovo = novaCP.strFavNaoFav + " à exclusão";
            }
            else //if (novaCP.strNomeTecnologia.IndexOf("incorporação") > 0)
            {
                strFavNaoFavNovo = novaCP.strFavNaoFav + " à incorporação";
            }


            strTemp = driver.FindElement(By.TagName("body")).GetAttribute("innerHTML");
            strTemp = strTemp.Replace("XX/2020", novaCP.strNuCP).Replace("#nomeTecnologia#", novaCP.strNomeTecnologia).Replace("#favNaofav#", strFavNaoFavNovo);

            pesquisaElement = driver.FindElement(By.TagName("body"));
            pesquisaElement.Clear();
            
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            string script1 = @"arguments[0].innerHTML='"+ strTemp +"'";
            js.ExecuteScript(script1, pesquisaElement);
            //voltar pro frame principal
            driver.SwitchTo().DefaultContent();
            //---------------------------------------------------------------------------------------------------------------
            

            //---------------------------------------------------------------------------------------------------------------
            iframeMsg = driver.FindElement(By.Id("mce_editor_1"));
            driver.SwitchTo().Frame(iframeMsg);

            strTemp = driver.FindElement(By.TagName("body")).GetAttribute("innerHTML");
            strTemp = strTemp.Replace("XX/2020", novaCP.strNuCP).Replace("#nomeTecnologia#", novaCP.strNomeTecnologia).Replace("#favNaofav#", strFavNaoFavNovo);

            pesquisaElement = driver.FindElement(By.TagName("body"));
            pesquisaElement.Clear();

            js = (IJavaScriptExecutor)driver;
            script1 = @"arguments[0].innerHTML='" + strTemp + "'";
            js.ExecuteScript(script1, pesquisaElement);
            //voltar pro frame principal
            driver.SwitchTo().DefaultContent();
            //---------------------------------------------------------------------------------------------------------------

            js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("return valida_campo();");
            Thread.Sleep(2000);
        }

        class FormulariosCP {
            string strNuTecnico;
            string strNuOpiniao;

            public string StrNuTecnico { get => strNuTecnico; set => strNuTecnico = value; }
            public string StrNuOpiniao { get => strNuOpiniao; set => strNuOpiniao = value; }
        }

        class ObjCP {

            public string strNuCP;
            public string dtInicio;
            public string dtFim;
            public string strNomeTecnologia;
            public string strNomeTecnologiaCurta;
            public string strFavNaoFav;
            public string iDNuTecnico;
            public string iDNuOpiniao;

        }

        class Teste
        {
            public Teste(string p1, string p2, string p3, string p4, string p5)
            {
                // TODO: Complete member initialization

                this.NomeC = p1;
                this.Nome1 = p2;
                this.Nome2 = p3;
                this.NomeDI = p4;
                this.NomeDF = p5;

            }

            public string NomeC { get; set; }
            public string Nome1 { get; set; }
            public string Nome2 { get; set; }
            public string NomeDI { get; set; }
            public string NomeDF { get; set; }

        }
    }
}
