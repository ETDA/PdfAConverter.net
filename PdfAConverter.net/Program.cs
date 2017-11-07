using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text.xml.xmp;

namespace PdfAConvertor
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFilePath = @"..\..\Resources/sample.pdf";
            string embbedFilePath = @"..\..\Resources /data.xml";
            string colorProfilePath = @"..\..\Resources /sRGB Color Space Profile.icm";
            string outputFilePath = "result.pdf";
            try
            {
                 CreatePDFA3Invoice(inputFilePath, embbedFilePath, outputFilePath, colorProfilePath);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static void CreatePDFA3Invoice(string pdfFilePath, string embbedFilePath, string outputPath, string colorProfilePath)
        {
            PdfReader reader = new PdfReader(pdfFilePath);
            MemoryStream stream = new MemoryStream();
            Document doc = new Document();
            PdfAWriter writer = CreatePDFAInstance(doc, reader, stream);


            // Create Output Intents
            ICC_Profile icc = ICC_Profile.GetInstance(File.ReadAllBytes(colorProfilePath));
            writer.SetOutputIntents("sRGB IEC61966-2.1", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfArray array = new PdfArray();
            writer.ExtraCatalog.Put(new PdfName("AF"), array);

            PdfFileSpecification contentSpec = EmbeddedAttachment(embbedFilePath, Path.GetFileName(embbedFilePath),
                    "text/xml", new PdfName("Alternative"), writer, "XML Data");
            array.Add(contentSpec.Reference);

            
            //string stringExchangeXMP = File.ReadAllText(@"..\..\Resources/EDocument_PDFAExtensionSchema.xml");
            //byte[] exchangeXMP = Encoding.ASCII.GetBytes(stringExchangeXMP);
            //writer.XmpMetadata = exchangeXMP;
            //XmpWriter xmp = new XmpWriter(stream, reader.Info);
            //writer.XmpMetadata = stream.ToArray();

            doc.Close();
            reader.Close();
            File.WriteAllBytes(outputPath, stream.ToArray());
        }

        private static PdfAWriter CreatePDFAInstance(Document doc, PdfReader originalDocument, Stream os)
        {
            PdfAWriter writer = PdfAWriter.GetInstance(doc, os, PdfAConformanceLevel.PDF_A_3U);
            writer.SetTagged();
            writer.PdfVersion = PdfWriter.VERSION_1_7;
            writer.CreateXmpMetadata();

            if (!doc.IsOpen())
                doc.Open();

            PdfContentByte cb = writer.DirectContent; // Holds the PDF data	
            PdfImportedPage page;
            int pageCount = originalDocument.NumberOfPages;
            for (int i = 0; i < pageCount; i++)
            {
                doc.NewPage();
                page = writer.GetImportedPage(originalDocument, i + 1);
                cb.AddTemplate(page, 0, 0);
            }
            return writer;
        }

        private static PdfFileSpecification EmbeddedAttachment(string filePath, string fileName, string mimeType,
            PdfName afRelationship, PdfAWriter writer, string description)
        {
            PdfDictionary parameters = new PdfDictionary();
            parameters.Put(PdfName.MODDATE, new PdfDate(File.GetLastWriteTime(filePath)));
            PdfFileSpecification fileSpec = PdfFileSpecification.FileEmbedded(writer, filePath, fileName, null, mimeType,
                    parameters, 0);
            fileSpec.Put(new PdfName("AFRelationship"), afRelationship);
            writer.AddFileAttachment(description, fileSpec);
            return fileSpec;
        }
    }
}
