﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace prxSearcher
{
    class ProxySearcher
    {
        private Thread mT;
        private bool mIsRun { get; set; }
        private Searcher mSearcher;
        private ProxiesList mSender;
        private string mSearchPhrase;
        private string mProxy;
        private Dictionary<string, Proxy> mPrxsDic;
        private int mPrxsCountNeed;

        public event EventHandler mKilled;
        public event EventHandler mPrxsLstUpdated;

        //methods
        public ProxySearcher(ProxiesList sender, Searcher sr, string SearchPhrase, int StartFromPage, ref List<Searcher> SearchersList, ref Dictionary<string, Proxy> PrxsDic, int PrxsCountNeed, string useProxy)
        {
            mSearcher = sr;
            mSearchPhrase = SearchPhrase;
            mSender = sender;
            mPrxsDic = PrxsDic;
            mPrxsCountNeed = PrxsCountNeed;
            mProxy = useProxy;                         

            mT = new Thread(new ThreadStart(ProxyToAssemble));
            mIsRun = true;
            mT.IsBackground = true;
            mT.Priority = ThreadPriority.Lowest;                                  
        }
                
        public void Start()
        {
            mT.Start();
        }

        public bool IsRun()
        {
            //return mT.ThreadState == ThreadState.Stopped;
            return mIsRun;
        }

        /// <summary>
        /// Search proxies and then parse them
        /// </summary>
        /// <param name="Prm">Id of thread</param>
        private void ProxyToAssemble()
        {
            StringBuilder txtQuery = new StringBuilder();
            txtQuery.Append(mSearcher.url);
            txtQuery.Append("&");
            txtQuery.Append(mSearcher.srchVar);
            txtQuery.Append("=");
            txtQuery.Append(mSearchPhrase.Replace(" ", mSearcher.spltr));
            txtQuery.Append("&");
            txtQuery.Append(mSearcher.pageVar);
            txtQuery.Append("=");

            while (mPrxsDic.Count < mPrxsCountNeed && mIsRun)
            {
                string html = "";
                string uri = txtQuery.ToString() + mSender.GetNewPageNumber(mSearcher).ToString();
                double t;
                if (Web_Client.Get(uri, mProxy, out html, out t))
                {
                    if (html.ToLower().Contains("captcha"))
                    {
                        StopLoading();
                    }
                    ParseProxies(html_parser.Matches(html, mSearcher.regexExpOfResults));
                }

                Thread.Sleep(200);
            }
            mIsRun = false;
            mKilled(this, EventArgs.Empty);
        }

        private void ParseProxies(string[] Urls)
        {
            foreach (string s in Urls)
            {
                if (!mIsRun)
                    break;

                string html = "";
                double t;

                if (Web_Client.Get(html_parser.ClearUrl(s), "", out html, out t))
                {
                    string[] proxies = html_parser.Matches(html, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:\d{2,5}");
                    if (proxies.Length > 0)
                    {
                        foreach (string p in proxies)
                        {
                            if (p != "")
                            {
                                if (!mPrxsDic.ContainsKey(p))
                                {
                                    Proxy newProxy = new Proxy();
                                    newProxy.adress = p;
                                    newProxy.latency = -1;
                                    Add(newProxy);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Add(object Value)
        {
            object lck = new object();
            lock (lck)
            {
                Proxy p = (Proxy)Value;
                if (!mPrxsDic.ContainsKey(p.adress))
                {
                    mPrxsDic.Add(p.adress, p);
                    mPrxsLstUpdated(this, EventArgs.Empty);
                }
            }
        }

        public void StopLoading()
        {
            mIsRun = false;
        }
    }
}
