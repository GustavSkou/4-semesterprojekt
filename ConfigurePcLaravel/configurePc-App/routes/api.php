<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\APIController;
use App\Http\Controllers\ComponentController;


/*
|--------------------------------------------------------------------------
| API Routes
|--------------------------------------------------------------------------
| All routes here are automatically prefixed with /api.
| Add routes here as user stories are implemented.
|
*/

Route::post('/production/command', [APIController::class, 'sendCommand']);
Route::post('/orders', [APIController::class, 'sendOrder']);

Route::get('components', [ComponentController::class, 'index']);