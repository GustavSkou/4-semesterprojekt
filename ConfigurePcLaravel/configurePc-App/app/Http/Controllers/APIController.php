<?php

namespace App\Http\Controllers;
use App\Models\Log;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Http;


class APIController extends Controller
{
    private function productionSystemUrl(string $path): string
    {
        $base = rtrim((string) env('PRODUCTION_API_URL'), '/');
        return $base . '/ProductionSystem/' . ltrim($path, '/');
    }
    
    public function startProduction()
    {
        $response = Http::post($this->productionSystemUrl('Start'));

        return response()->json($response->json(), $response->status());
    }

    public function stopProduction()
    {
        $response = Http::post($this->productionSystemUrl('Stop'));

        return response()->json($response->json(), $response->status());
    }

    public function resetProduction()
    {
        $response = Http::post($this->productionSystemUrl('Reset'));

        return response()->json($response->json(), $response->status());
    }

    public function sendCommand(Request $request)
    {
        // Validating incoming request data
        $validatedData = $request->validate([
            'command' => 'required|string',
            'parameters' => 'nullable|array',
        ]);

        // Processes the command and parameters
        $command = $validatedData['command'];
        $parameters = $validatedData['parameters'] ?? [];

        // Success response
        return response()->json([
            'message' => 'Command received successfully',
            'command' => $command,
            'parameters' => $parameters,
        ], 200);
    }

    public function sendOrder(Request $request) {
        $validated = $request->validate([
            'id' => 'required|integer',
            'trayIds' => 'required|array',
            'trayIds.*' => 'integer',
        ]);

        $url = $this->productionSystemUrl('Command');

        $response = Http::post($url, [
            'Name' => 'order',
            'Parameters' => [
                'id'    => $validated['id'],
                'items' => $validated['trayIds'],
            ]
        ]);

        return response()->json($response->json(), $response->status());

    }

    public function getQueueSnapshot()
    {
        $response = Http::get($this->productionSystemUrl('Queue'));
        return response()->json($response->json(), $response->status());
    }

    public function getMachinesSnapshot()
    {
        $response = Http::get($this->productionSystemUrl('Machines'));
        return response()->json($response->json(), $response->status());
    }

    public function getOrderStatusSnapshot(int $orderId)
    {
        $response = Http::get($this->productionSystemUrl("OrderStatus/{$orderId}"));
        return response()->json($response->json(), $response->status());
    }

    public function getProductionLogs(Request $request)
    {
        $limit = min(max((int) $request->query('limit', 200), 1), 1000);

        $logs = Log::with(['level', 'source', 'type'])
            ->orderByDesc('timestamp')
            ->limit($limit)
            ->get()
            ->map(function (Log $log) {
                return [
                    'id' => 'LOG-' . $log->id,
                    'timestamp' => $log->timestamp,
                    'description' => (string) $log->description,
                    'level' => strtolower((string) optional($log->level)->name),
                    'source' => (string) optional($log->source)->name,
                    'type' => (string) optional($log->type)->name,
                ];
            });

        return response()->json(['logs' => $logs]);
    }
}