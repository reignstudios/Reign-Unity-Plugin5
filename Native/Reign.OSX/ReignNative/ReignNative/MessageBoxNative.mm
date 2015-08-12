//
//  ReignNative.m
//  ReignNative
//
//  Created by Andrew Witte on 1/27/15.
//  Copyright 2015 __MyCompanyName__. All rights reserved.
//

#import "MessageBoxNative.h"


@implementation MessageBoxNative

- (id)init
{
    self = [super init];
    if (self) {
        // Initialization code here.
    }
    
    return self;
}

- (void)dealloc
{
    [super dealloc];
}

- (int)Show:(NSString*)title message:(NSString*)message type:(int)type
{
    NSAlert *alert = [NSAlert alertWithMessageText:@"Testing"
                                     defaultButton:@"OK"
                                   alternateButton:@"Cancel"
                                       otherButton:nil
                         informativeTextWithFormat:@""];
    
    NSTextField *input = [[NSTextField alloc] initWithFrame:NSMakeRect(0, 0, 200, 24)];
    [input setStringValue:@"Testing2"];
    [input autorelease];
    [alert setAccessoryView:input];
    NSInteger button = [alert runModal];
    if (button == NSAlertDefaultReturn)
    {
        [input validateEditing];
        return 1;//[input stringValue];
    }
    else if (button == NSAlertAlternateReturn)
    {
        return 0;
    }
    else
    {
        NSAssert1(NO, @"Invalid input dialog button %d", button);
        return 0;
    }
    
    /*NSAlert* alert = nil;
    if (type == 0)
    {
        //alert = [[NSAlert alloc] initWithTitle:title message:message delegate:self cancelButtonTitle:@"Ok" otherButtonTitles:nil];
    }
    else
    {
        //alert = [[NSAlert alloc] initWithTitle:title message:message delegate:self cancelButtonTitle:@"Cancel" otherButtonTitles:@"Ok", nil];
    }
    
    [alert show];
    [alert release];*/
}
@end

//============================
// Unity C Link
//============================
static MessageBoxNative* native = nil;

extern "C"
{
    void MessageBoxInit()
    {
        if (native == nil) native = [[MessageBoxNative alloc] init];
    }
    
    int MessageBoxShow(const char* title, const char* message, int type)
    {
        return [native Show:nil message:nil type:type];
    }
}