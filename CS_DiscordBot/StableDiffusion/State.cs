namespace LiteBot.StableDiffusion;

public record State(bool Skipped, bool Interrupted, int SamplingStep, int SamplingSteps);