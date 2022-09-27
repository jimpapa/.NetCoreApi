using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WebApplication1.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using HtmlAgilityPack;
using iText.Html2pdf;
using System.Drawing;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public PatientController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        [HttpGet]
        public JsonResult Get()
        {
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SQLConfiguration");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand("[dbo].[patients_search]", myCon))
                {
                    myCommand.CommandType  = CommandType.StoredProcedure; 
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult(table);
        }

        [HttpPost]
        public JsonResult Post(Patient patient)
        {
            string query = @"
                            insert into dbo.Patients
                           (Lastname,Address,Birthdate)
                            values (@Lastname,@Address,@Birthdate)  
                            ";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SQLConfiguration");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand("[dbo].[patient_insert]", myCon))
                {
                    myCommand.CommandType = CommandType.StoredProcedure; 
                    myCommand.Parameters.AddWithValue("@Lastname", patient.Lastname );
                    myCommand.Parameters.AddWithValue("@Firstname", patient.Firstname );
                    myCommand.Parameters.AddWithValue("@Birthdate", patient.Birthdate);
                    myCommand.Parameters.AddWithValue("@Address", patient.Address);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult("Added Successfully");
        }


        [HttpPut]
        public JsonResult Put(Patient patient)
        {
            string query = @"
                            update dbo.Patients
                           set Lastname= @Lastname,
                            Firstname= @Firstname,
                            Address=@Address,
                            Birthdate=@Birthdate
                            where Id=@Id
                            ";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SQLConfiguration");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@Id", patient.Id);
                    myCommand.Parameters.AddWithValue("@Lastname", patient.Lastname);
                    myCommand.Parameters.AddWithValue("@Firstname", patient.Firstname);
                    myCommand.Parameters.AddWithValue("@Address", patient.Address);
                    myCommand.Parameters.AddWithValue("@Birthdate", patient.Birthdate);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult("Updated Successfully");
        }

        [HttpDelete("{id}")]
        public JsonResult Delete(int id)
        {
            string query = @"
                           Update dbo.Patients
                            Set Active = 0
                            where Id=@Id
                            ";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SQLConfiguration");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@Id", id);

                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult("Deleted Successfully");
        }

        [Route("SaveFile")]
        [HttpPost]
        public JsonResult SaveFile()
        {
            try
            {
                var httpRequest = Request.Form;
                var postedFile = httpRequest.Files[0];
                string filename = postedFile.FileName;
                var physicalPath = _env.ContentRootPath + "/Photos/" + filename;

                using(var stream = new FileStream(physicalPath ,FileMode.Create ) )
                {
                    postedFile.CopyTo(stream);
                }

                return new JsonResult(filename);
            } catch (Exception)
            {
                return new JsonResult("anonymous.png"); 
            }
        }

        [Route("SaveHtmlFile")]
        [HttpPost]
        public JsonResult SaveHtmlFile()
        {
            try
            {
                var httpRequest = Request.Form;
                var postedFile = httpRequest.Files[0];
                string filename = postedFile.FileName;
                string HtmlFilePath = "C:\\Files\\TestSigns\\HtmlFiles\\" + filename;
                string PdfFilePath = "C:\\Files\\TestSigns\\PdfFiles\\";

                using (var stream = new FileStream(HtmlFilePath, FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                }

                // Create Html Document
                var doc = new HtmlDocument();
                doc.Load(HtmlFilePath, true);
                // Object html
                var nodehtml = doc.DocumentNode.SelectSingleNode("//html");
                // Object img
                var nodeimg = doc.DocumentNode.SelectNodes("//img");
                // Object button
                var nodebtn = doc.DocumentNode.SelectNodes("//button");
                // Object footer
                var nodefooter = doc.DocumentNode.SelectNodes("//footer");

                TextWriter writer = System.IO.File.CreateText(HtmlFilePath); 

                string InputTempPDF = PdfFilePath + Path.GetFileNameWithoutExtension(HtmlFilePath) + "_temp.pdf";
                string OutputFile = PdfFilePath + Path.GetFileNameWithoutExtension(HtmlFilePath) + ".pdf";

                if (nodebtn != null)
                {
                   // '''''Delete all buttons Without Signs'''''
                   foreach (HtmlNode htmlbtn in nodebtn)
                   {
                       if (htmlbtn.GetAttributeValue("name", "").Contains("$$sign"))
                           htmlbtn.Remove();
                   }
                }
                string imgpathSource = _env.ContentRootPath + "/Photos/sign_image.png";
                byte[] imgbytes = System.IO.File.ReadAllBytes(imgpathSource);
               
                 string image_string = Convert.ToBase64String(imgbytes);
                 string biometrics_string = "RlOY0BcBGAEwFS8FBwkKAwYIBA0MTxQdHBobGFQXFjASARAJhXg47sxmEJB5Ixcu/FYZLz0BDwBBAGQAZABpAHQAaQBvAG4AYQBsAEQAYQB0AGEADwBDAHIAZQBhAHQAZQBkACAAYgB5ACAAVABNAFMABQoBBQCAS6CNBgIABwoBBQDwLqCNBgIBCQgBBQAA6AcEAAoKApkB8xMJBAJLqAMOApkBAQgDB3UABDgAMQAGygECmQGyEAkKwQG+b9t20bTuO/8j0/e+n9H+AeD4XB4JgtkaWpwIQfh8GEUpJESYhwnSdhxGoYpalERw9DMKQhhiEYLAABH9/b73mdtz/CZ5gVs2De+O59r2+c53XpfP+4AAsEwfCyOxLFcYhpm6cp0liSpCj2WBiGQYBXkyP4xiUGYOAB+Xt+Lx2rY/eeG5JoOzch3nygYGgqkIU5mnsgCIIKexxEwN4tigM4zCqIgXg8AQA+Hp9XvGg5zmudaVsnB9F4PufmA4LhQACLgBApkBgREJCa8Bvg8JBULJaKxMIg5GAmDwXCIJAP29XO2OVveCw2k2m94epz+cz2k1m03vF5/Y8Hn9vz/H9AAHBYSDQmGI+Jh3SysW6UPhmKI9FwfCgOBQABgE/Xs73S5HA3m63G343O6fY42/3vA43O6/g9Hw/QACAaExGNCLVSiTyWQxxLhHGwlCX99PU5u722s0ef2/J6vd6XV7Pl9v3AIA/YXFxMOJ7QYBRX4Qh9ORnLhUJ5IJQAQOApkBAQgCB3UAMNABEQANyQECmQG2AgcLwAHmAwBABAB//AOAAA4AP8AA/yAQAoIAHD/YAQBAHAgBYC4EwHgJAMAUAgBQBAFAIBAGBAEDAD6AEAAAYAgBgFAEBAB8/+AD/3+v6gf4W/6f6/+AEAYBABgC///7/4AgEgLAeA0AwBABAAAH/3+X5vr/O+b9H7v6v9AIA2CsF4KwSAfAgl+ggAqAoAoEAHL/v+/8/z/R+n9H7/9/3+57Tw/ZPXjQAfkBgZA6BAuAAOB//7/z/X9v4Pr/KA/B/fA9r6QMCAEFAP8HAAAATwUE0k1i5RQIAQQAgEsA8C4dFhUGqKFZixA5BgAJD/4AAQZUHPf2dA4cKCdNaWNyb3NvZnQ7J1dpbmRvd3MgMTAnOzs7MTAuMC4xOTA0MS42NjIaISBTVFU7J1NUVS00MzAnOzEuMC41OzA7OEhaUTAwNTQ0MhsWFTAwNzA7d2l6YXJkO3dhY29tO1NUVRgH7tKElwa0AFQlJGI4NTIzZWJiLTE4YjctNDc4YS1hOWRlLTg0OWZlMjI2YjAwMhcUE1ZvbHVwdGF0ZW0gcmVydW0gZGkWbWxJVkZfRjI2Z3JfzpXOnc6kzp/Om86XIM6gzpHOoc6RzpTOn86jzpfOoyDOms6hzqXOn86jzqXOnc6kzpfOoc6XzpzOlc6dzp/OpSDOks6Zzp/Om86fzpPOmc6azp/OpSDOpc6bzpnOms6fzqU=";

                if (nodeimg != null)
                {
                    // '''''Delete all buttons Without Signs'''''
                         foreach (HtmlNode htmlimg in nodeimg)
                         {
                             if (htmlimg.GetAttributeValue("src", "").Contains("$$sign"))
                             {
                                 htmlimg.Attributes.Remove("hidden");
                                 htmlimg.SetAttributeValue("src", "data:image/jpg;base64," + image_string);
                             }            
                         }
                }
                if (nodefooter != null)
                {
                    foreach (HtmlNode htmlfooter in nodefooter)
                    {
                        if (htmlfooter.OuterHtml.Contains("$$sigdata"))
                            htmlfooter.InnerHtml = biometrics_string;
                    }
                }

                if (doc.DocumentNode.SelectNodes("//input") != null)
                {
                    // '''''Convert input Date -> Text bug itextSharp '''''
                    foreach (HtmlNode htmlinput in doc.DocumentNode.SelectNodes("//input"))
                    {
                        if (htmlinput.GetAttributeValue("type", "") == "date")
                            htmlinput.SetAttributeValue("type", "text");
                    }
                }

                if (doc.DocumentNode.SelectNodes("//div") != null)
                {
                    // '''''Delete class Page From Div -> Page bug itextSharp '''''
                    foreach (HtmlNode htmldiv in doc.DocumentNode.SelectNodes("//div"))
                    {
                        if (htmldiv.GetAttributeValue("class", "") == "page")
                            htmldiv.RemoveClass();
                    }
                }

           
                nodehtml.WriteTo(writer);
                writer.Close();

                using (FileStream pdfDest = new FileStream(InputTempPDF, FileMode.OpenOrCreate))
                {
                    using (FileStream htmlSource = new FileStream(HtmlFilePath, FileMode.Open))
                    {
                        ConverterProperties converterProperties = new ConverterProperties();
                        // Dim fontprovider As FontProvider = New DefaultFontProvider(True, True, False)
                        // converterProperties.SetFontProvider(FontProvider)
                        // converterProperties.SetImmediateFlush(False)
                        HtmlConverter.ConvertToPdf(htmlSource, pdfDest, converterProperties);
                    }
                }

                // ''''''''''''''Lock PDF After Creation'''''''''''''''
                using (FileStream input = new FileStream(InputTempPDF, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (FileStream output = new FileStream(OutputFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                    {
                        using (iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(input))
                        {
                            iTextSharp.text.pdf.PdfEncryptor.Encrypt(reader, output, true, null/* TODO Change to default(_) if this is not a reference type */, null/* TODO Change to default(_) if this is not a reference type */, iTextSharp.text.pdf.PdfWriter.ALLOW_DEGRADED_PRINTING);
                        }
                    }
                }
                // ''''''''''''''/Lock PDF After Creation'''''''''''''''

                // ''''''''''''''Delete Temp Html - PDF Files'''''''''''''''
                System.IO.File.Delete(HtmlFilePath);
                System.IO.File.Delete(InputTempPDF);

                return new JsonResult(OutputFile);
            }
            catch (Exception ex)
            {
                return new  JsonResult(ex.Message);
            }
        }
    }
}
