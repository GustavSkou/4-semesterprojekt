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
Route::get('/production/queue', [APIController::class, 'getQueueSnapshot']);
Route::get('/production/machines', [APIController::class, 'getMachinesSnapshot']);
Route::get('/production/order-status/{orderId}', [APIController::class, 'getOrderStatusSnapshot']);
Route::get('/production/logs', [APIController::class, 'getProductionLogs']);

Route::get('components', [ComponentController::class, 'index']);