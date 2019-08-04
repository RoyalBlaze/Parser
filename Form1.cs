using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Abot.Poco;
using Abot.Crawler;
using Abot.Core;
using System.Configuration;
using System.Net;
using System.Data.SqlClient;

namespace PageParser
{
    public partial class Form1 : Form
    {

        public static string ConnectionString;
        public Form1()
        {
            InitializeComponent();
            //ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=ParsedSites;Integrated Security=True";
            ConnectionString = @"Data Source=.\SQLEXPRESS;AttachDbFilename=C:\Program Files\Microsoft SQL Server\MSSQL14.SQLEXPRESS\MSSQL\DATA\ParsedSites.mdf;Integrated Security=True";

        }

        private void button_crawl_Click(object sender, EventArgs e)
        {
            CrawlConfiguration crawlConfig = new CrawlConfiguration();
            crawlConfig.CrawlTimeoutSeconds = 100;
            crawlConfig.MaxConcurrentThreads = 10;
            crawlConfig.MaxPagesToCrawl = 1000;
            crawlConfig.UserAgentString = "abot v1.0 http://code.google.com/p/abot";

            PoliteWebCrawler crawler = new PoliteWebCrawler(crawlConfig, null, null, null, null, null, null, null, null);
            crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
            crawler.PageCrawlCompleted += crawler_ProcessPageCrawlCompleted;
            crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
            crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;
            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;



            CrawlResult result = crawler.Crawl(new Uri("https://belaruspartisan.by/")); //This is synchronous, it will not go to the next line until the crawl has completed

            

            if (result.ErrorOccurred)
                MessageBox.Show("Crawl of " + result.RootUri.AbsoluteUri + " completed with error: " + result.ErrorException.Message);
            // Console.WriteLine("Crawl of {0} completed with error: {1}", result.RootUri.AbsoluteUri, result.ErrorException.Message);
            else
                MessageBox.Show("Crawl of " + result.RootUri.AbsoluteUri + " completed without error.");
            //Console.WriteLine("Crawl of {0} completed without error.", result.RootUri.AbsoluteUri);

        }

        void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            //MessageBox.Show("About to crawl link " + pageToCrawl.Uri.AbsoluteUri + " which was found on page " + pageToCrawl.ParentUri.AbsoluteUri);
            //Console.WriteLine("About to crawl link {0} which was found on page {1}", pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri);
        }

        void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            
            var htmlAgilityPackDocument = crawledPage.HtmlDocument; //Html Agility Pack parser
            var angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument; //AngleSharp parser
            var url = crawledPage.Uri;
            var htmltext = crawledPage.Content.Text;
            var c = htmlAgilityPackDocument.DocumentNode.SelectSingleNode("//h1[@class = 'name']"); // DONE!!
            var b = htmlAgilityPackDocument.DocumentNode.SelectSingleNode("//div[@class='news-detail']");
            if (c != null)
            {
                if (b != null)
                {
                    if(htmltext!= null)
                    { 
                    Int64 id;
                    var sql = "INSERT INTO Pages (PageURL,HTMLText,PageText,PageTitle,Id_Website) " +
               "VALUES (@PageURL,@HTMLText,@PageText,@PageTitle,@Id_Website); SET @Id_Page=SCOPE_IDENTITY()";
                        using (SqlConnection sqlConn = new SqlConnection(ConnectionString))
                        {

                            sqlConn.Open();
                            SqlDataAdapter da1 = new SqlDataAdapter();
                            using (var cmd = new SqlCommand(sql, sqlConn))
                            {
                                cmd.Parameters.AddWithValue("@PageURL", url.ToString());
                                cmd.Parameters.AddWithValue("@HTMLText", htmltext.ToString());
                                cmd.Parameters.AddWithValue("@PageText", b.InnerText.ToString());
                                cmd.Parameters.AddWithValue("@PageTitle", c.InnerText.ToString());
                                cmd.Parameters.AddWithValue("@Id_Website", 1);

                                SqlParameter idParam = new SqlParameter
                                {
                                    ParameterName = "@Id_Page",
                                    SqlDbType = SqlDbType.Int,
                                    Direction = ParameterDirection.Output, // параметр выходной
                                };
                                cmd.Parameters.Add(idParam);
                                cmd.ExecuteNonQuery();
                                cmd.CommandText = "SELECT @@IDENTITY";
                                id = Convert.ToInt64(cmd.ExecuteScalar());
                                //MessageBox.Show("insert status: done!");
                            }
                            sqlConn.Close();
                        }
                    }
                }

            }
        }

        void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            //MessageBox.Show("Did not crawl the links on page " + crawledPage.Uri.AbsoluteUri + " due to " + e.DisallowedReason);
            //Console.WriteLine("Did not crawl the links on page {0} due to {1}", crawledPage.Uri.AbsoluteUri, e.DisallowedReason);
        }

        void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            //MessageBox.Show("Did not crawl page " + pageToCrawl.Uri.AbsoluteUri + " due to " + e.DisallowedReason);
            //Console.WriteLine("Did not crawl page {0} due to {1}", pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason);
        }

        private void dBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var a = new ParsedPages();
            a.ShowDialog();
        }
    }
}
