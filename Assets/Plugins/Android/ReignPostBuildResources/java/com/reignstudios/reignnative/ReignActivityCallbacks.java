package com.reignstudios.reignnative;

import android.content.Intent;

public interface ReignActivityCallbacks
{
	boolean onActivityResult(int requestCode, int resultcode, Intent intent);
	void onPause();
	void onResume();
}
