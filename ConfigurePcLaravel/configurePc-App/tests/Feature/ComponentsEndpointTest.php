<?php

namespace Tests\Feature;

use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

class ComponentsEndpointTest extends TestCase
{
    use RefreshDatabase;

    public function test_components_endpoint_returns_seeded_catalog_with_relationship_data(): void
    {
        $this->seed();

        $response = $this->getJson('/api/components');

        $response->assertOk();
        $response->assertJsonCount(31);
        $response->assertJsonStructure([
            '*' => [
                'id',
                'name',
                'brand',
                'tray_id',
                'price',
                'category' => [
                    'id',
                    'name',
                ],
                'specifications' => [
                    '*' => ['id', 'name', 'value', 'component_id'],
                ],
                'wattage_lists' => [
                    '*' => ['id', 'wattage', 'component_id'],
                ],
            ],
        ]);
    }

    public function test_components_endpoint_exposes_compatibility_metadata_for_frontend_checks(): void
    {
        $this->seed();

        $components = $this->getJson('/api/components')->json();

        $cpu = collect($components)->firstWhere('tray_id', 1);
        $case = collect($components)->firstWhere('tray_id', 24);
        $cooler = collect($components)->firstWhere('tray_id', 30);

        $this->assertNotNull($cpu);
        $this->assertNotNull($case);
        $this->assertNotNull($cooler);

        $this->assertTrue(collect($cpu['specifications'])->contains(
            fn (array $spec) => $spec['name'] === 'Socket' && $spec['value'] === 'AM5'
        ));
        $this->assertTrue(collect($cpu['specifications'])->contains(
            fn (array $spec) => $spec['name'] === 'TDP' && $spec['value'] === '105W'
        ));
        $this->assertSame(105, $cpu['wattage_lists'][0]['wattage']);

        $this->assertTrue(collect($case['specifications'])->contains(
            fn (array $spec) => $spec['name'] === 'Form Factor' && $spec['value'] === 'ATX'
        ));
        $this->assertTrue(collect($case['specifications'])->contains(
            fn (array $spec) => $spec['name'] === 'Form Factor' && $spec['value'] === 'mATX'
        ));

        $coolerSockets = collect($cooler['specifications'])
            ->where('name', 'Socket')
            ->pluck('value')
            ->all();

        $this->assertEqualsCanonicalizing(['AM4', 'AM5', 'LGA1700'], $coolerSockets);
    }
}
