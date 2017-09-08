//
//  Request.m
//  HttpRequest
//
//  Created by mtakagi on 2017/09/08.
//  Copyright © 2017年 mtakagi. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "HttpRequest.h"

extern "C" {
    void * HttpRequest_Create(const char *, void *, ResponseCallback, DownloadPorgressCallback,
                              RequestFailedCallback,RequestFinishedCallback);
    void HttpRequest_Start(void * request);
    void HttpRequest_Cancel(void * request);
    void HttpRequest_Dispose(void * request);
}

void * HttpRequest_Create(const char * url, void *context,
                          ResponseCallback responseCallback,
                          DownloadPorgressCallback downloadProgressCallback,
                          RequestFailedCallback requestFailedCallback,
                          RequestFinishedCallback requestFinishedCallback) {
    HttpRequest *request = [[HttpRequest alloc] initWithURL:[NSString stringWithCString:url
                                                                               encoding:NSUTF8StringEncoding]];
    request.context = context;
    request.responseCallback = responseCallback;
    request.downloadProgressCallback = downloadProgressCallback;
    request.requestFailedCallback = requestFailedCallback;
    request.requestFinishedCallback = requestFinishedCallback;

    return (void *)request;
}

void HttpRequest_Start(void * request) {
    [(HttpRequest *)request start];
}

void HttpRequest_Cancel(void * request) {
    [(HttpRequest *)request cancel];
}

void HttpRequest_Dispose(void * request) {
    [(HttpRequest *)request release];
}
