<?php

namespace Tests\Feature;

use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Http;
use Tests\TestCase;

class FunctionalRequirementsTest extends TestCase
{
    use RefreshDatabase;

    public function test_F01_user_can_configure_a_computer_through_gui(): void
    {
        $this->markTestSkipped(
            'F01 dækkes i frontend-testene med Vitest; denne Laravel-suite dækker kun backend-adfærd.'
        );
    }

    public function test_F02_validates_incompatible_components(): void
    {
        $this->markTestSkipped(
            'F02 dækkes i frontend-testene for checkCompatibility() og ConfigurePage; denne Laravel-suite dækker kun backend-adfærd.'
        );
    }

    public function test_F03_sends_configuration_payload_to_production_system(): void
    {
        $this->setProductionApiUrl('http://example.test');
        $this->seed();
        Http::fake();

        $this->postJson('/api/orders', [
            'name' => 'Jane Doe',
            'email' => 'jane@example.com',
            'address' => 'Main Street 1',
            'id' => 3001,
            'trayIds' => [10, 11, 24],
        ])->assertOk();

        Http::assertSent(function ($request) {
            return str_ends_with($request->url(), '/ProductionSystem/Command')
                && $request['Name'] === 'order'
                && $request['Parameters']['id'] === 3001
                && $request['Parameters']['items'] === [10, 11, 24];
        });
    }

    public function test_F08_updates_operator_interface_in_real_time(): void
    {
        $this->markTestSkipped(
            'F08 dækkes i frontend-testene for ProductionContext/EventSource; denne Laravel-suite dækker kun backend-adfærd.'
        );
    }

    public function test_F12_operator_can_log_in(): void
    {
        $this->markTestSkipped(
            'F12 dækkes i frontend-testene for OperatorLoginPage; denne Laravel-suite dækker kun backend-adfærd.'
        );
    }

    public function test_F18_requires_customer_information_when_ordering(): void
    {
        $this->setProductionApiUrl('http://example.test');
        Http::fake([
            '*' => Http::response(['ok' => true], 200),
        ]);

        $this->postJson('/api/orders', [
            'id' => 1801,
            'trayIds' => [10, 11],
        ])->assertStatus(422)
            ->assertJsonValidationErrors(['name', 'email', 'address']);
    }

    public function test_F19_rejects_order_when_component_is_out_of_stock(): void
    {
        $this->setProductionApiUrl('http://example.test');
        Http::fake([
            '*' => Http::response(['ok' => true], 200),
        ]);

        $this->postJson('/api/orders', [
            'id' => 1901,
            'trayIds' => [999],
        ])->assertStatus(422)
            ->assertJsonValidationErrors(['trayIds.0']);
    }

    private function setProductionApiUrl(string $url): void
    {
        putenv("PRODUCTION_API_URL={$url}");
        $_ENV['PRODUCTION_API_URL'] = $url;
        $_SERVER['PRODUCTION_API_URL'] = $url;
    }
}
