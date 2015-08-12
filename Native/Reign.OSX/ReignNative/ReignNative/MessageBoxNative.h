//
//  ReignNative.h
//  ReignNative
//
//  Created by Andrew Witte on 1/27/15.
//  Copyright 2015 __MyCompanyName__. All rights reserved.
//

#import <Foundation/Foundation.h>


@interface MessageBoxNative : NSObject {
@private
    
}

- (int)Show:(NSString*)title message:(NSString*)message type:(int)type;
@end
