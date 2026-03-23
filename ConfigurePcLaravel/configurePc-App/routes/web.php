<?php

use Illuminate\Support\Facades\Route;

// Catch-all: let React Router handle all frontend routes.
// Any path that isn't an /api/* route gets the SPA shell.
Route::get('/{any?}', function () {
    return view('app');
})->where('any', '^(?!api).*$');

